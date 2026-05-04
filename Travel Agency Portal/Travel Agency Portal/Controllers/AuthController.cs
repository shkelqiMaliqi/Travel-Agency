using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Travel_Agency_Portal.Models;
using Travel_Agency_Portal.Services;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[EnableRateLimiting("AuthPolicy")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(IConfiguration configuration, JwtTokenService jwtTokenService)
    {
        _configuration = configuration;
        _jwtTokenService = jwtTokenService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return BadRequest(new { message = "Passwords do not match." });
        }

        if (!PasswordPolicy.IsValid(request.Password))
        {
            return BadRequest(new { message = PasswordPolicy.Message });
        }

        const string existsQuery = "SELECT COUNT(1) FROM dbo.Users WHERE U_Email = @U_Email OR U_Username = @U_Username";
        var existingUsers = ExecuteScalar(existsQuery,
            new SqlParameter("@U_Email", request.U_Email),
            new SqlParameter("@U_Username", request.U_Username));

        if (existingUsers > 0)
        {
            return Conflict(new { message = "A user with this email or username already exists." });
        }

        var hashedPassword = PasswordHasher.Hash(request.Password);
        const string publicRegistrationRole = "user";

        const string insertQuery = @"
            INSERT INTO dbo.Users (U_Name, U_Surname, U_Email, U_Username, U_Phone, U_Password, U_RepeatPassword, U_Type)
            OUTPUT INSERTED.U_Id
            VALUES (@U_Name, @U_Surname, @U_Email, @U_Username, @U_Phone, @U_Password, @U_RepeatPassword, @U_Type)";

        var userId = ExecuteScalar(insertQuery,
            new SqlParameter("@U_Name", request.U_Name),
            new SqlParameter("@U_Surname", request.U_Surname),
            new SqlParameter("@U_Email", request.U_Email),
            new SqlParameter("@U_Username", request.U_Username),
            new SqlParameter("@U_Phone", (object?)request.U_Phone ?? DBNull.Value),
            new SqlParameter("@U_Password", hashedPassword),
            new SqlParameter("@U_RepeatPassword", hashedPassword),
            new SqlParameter("@U_Type", publicRegistrationRole));

        var user = new Users
        {
            U_Id = userId,
            U_Name = request.U_Name,
            U_Surname = request.U_Surname,
            U_Email = request.U_Email,
            U_Username = request.U_Username,
            U_Phone = request.U_Phone,
            U_Type = publicRegistrationRole
        };

        return Ok(_jwtTokenService.CreateToken(user));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        const string query = @"
            SELECT TOP 1 U_Id, U_Name, U_Surname, U_Email, U_Username, U_Phone, U_Password, U_RepeatPassword, U_Type
            FROM dbo.Users
            WHERE U_Email = @UserNameOrEmail OR U_Username = @UserNameOrEmail";

        var table = ExecuteQuery(query, new SqlParameter("@UserNameOrEmail", request.UserNameOrEmail));
        if (table.Rows.Count == 0)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var row = table.Rows[0];
        var storedHash = row["U_Password"]?.ToString() ?? string.Empty;
        if (!PasswordHasher.Verify(request.Password, storedHash))
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var user = new Users
        {
            U_Id = Convert.ToInt32(row["U_Id"]),
            U_Name = row["U_Name"]?.ToString() ?? string.Empty,
            U_Surname = row["U_Surname"]?.ToString() ?? string.Empty,
            U_Email = row["U_Email"]?.ToString() ?? string.Empty,
            U_Username = row["U_Username"]?.ToString() ?? string.Empty,
            U_Phone = row["U_Phone"] == DBNull.Value ? null : row["U_Phone"]?.ToString(),
            U_Type = row["U_Type"]?.ToString() ?? "user"
        };

        return Ok(_jwtTokenService.CreateToken(user));
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        const string userQuery = "SELECT COUNT(1) FROM dbo.Users WHERE U_Email = @U_Email";
        var existingUsers = ExecuteScalar(userQuery, new SqlParameter("@U_Email", request.Email));

        if (existingUsers == 0)
        {
            return NotFound(new { message = "No account exists with this email." });
        }

        var resetCode = Random.Shared.Next(100000, 999999).ToString();
        var resetCodeHash = PasswordHasher.HashToken(resetCode);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(15);

        const string expireOldCodesQuery = @"
            UPDATE dbo.Password_Reset_Codes
            SET Is_Used = 1
            WHERE U_Email = @U_Email AND Is_Used = 0";

        ExecuteNonQuery(expireOldCodesQuery, new SqlParameter("@U_Email", request.Email));

        const string insertCodeQuery = @"
            INSERT INTO dbo.Password_Reset_Codes (U_Email, Reset_Code_Hash, Expires_At, Is_Used, Created_At)
            VALUES (@U_Email, @Reset_Code_Hash, @Expires_At, 0, SYSUTCDATETIME())";

        ExecuteNonQuery(insertCodeQuery,
            new SqlParameter("@U_Email", request.Email),
            new SqlParameter("@Reset_Code_Hash", resetCodeHash),
            new SqlParameter("@Expires_At", expiresAtUtc));

        return Ok(new
        {
            message = "Reset code generated. Copy this code and use it to set a new password.",
            resetCode,
            expiresAtUtc
        });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return BadRequest(new { message = "Passwords do not match." });
        }

        if (!PasswordPolicy.IsValid(request.NewPassword))
        {
            return BadRequest(new { message = PasswordPolicy.Message });
        }

        var resetCodeHash = PasswordHasher.HashToken(request.ResetCode.Trim());
        const string codeQuery = @"
            SELECT TOP 1 Reset_Id
            FROM dbo.Password_Reset_Codes
            WHERE U_Email = @U_Email
              AND Reset_Code_Hash = @Reset_Code_Hash
              AND Is_Used = 0
              AND Expires_At >= SYSUTCDATETIME()
            ORDER BY Created_At DESC";

        var resetId = ExecuteScalar(codeQuery,
            new SqlParameter("@U_Email", request.Email),
            new SqlParameter("@Reset_Code_Hash", resetCodeHash));

        if (resetId == 0)
        {
            return BadRequest(new { message = "Reset code is invalid or expired." });
        }

        var hashedPassword = PasswordHasher.Hash(request.NewPassword);
        const string updateQuery = @"
            UPDATE dbo.Users
            SET U_Password = @U_Password,
                U_RepeatPassword = @U_Password
            WHERE U_Email = @U_Email";

        ExecuteNonQuery(updateQuery,
            new SqlParameter("@U_Email", request.Email),
            new SqlParameter("@U_Password", hashedPassword));

        const string markCodeUsedQuery = "UPDATE dbo.Password_Reset_Codes SET Is_Used = 1 WHERE Reset_Id = @Reset_Id";
        ExecuteNonQuery(markCodeUsedQuery, new SqlParameter("@Reset_Id", resetId));

        return Ok(new { message = "Password reset successfully. You can now sign in." });
    }

    private DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
    {
        var table = new DataTable();
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        using var adapter = new SqlDataAdapter(command);
        adapter.Fill(table);

        return table;
    }

    private int ExecuteScalar(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        var result = command.ExecuteScalar();

        return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    private int ExecuteNonQuery(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        return command.ExecuteNonQuery();
    }
}
