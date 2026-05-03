using System.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public UsersController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Authorize(Roles = "admin")]
    [HttpGet]
    public IActionResult GetUsers()
    {
        const string query = @"SELECT U_Id, U_Name, U_Surname, U_Email, U_Username, U_Phone, U_Type FROM dbo.Users ORDER BY U_Id DESC";
        return Ok(ExecuteUsersQuery(query));
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public IActionResult GetUser(int id)
    {
        if (!CanAccessUser(id))
        {
            return Forbid();
        }

        const string query = @"SELECT U_Id, U_Name, U_Surname, U_Email, U_Username, U_Phone, U_Type FROM dbo.Users WHERE U_Id = @U_Id";
        var result = ExecuteUsersQuery(query, new SqlParameter("@U_Id", id));

        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public IActionResult UpdateUser(int id, [FromBody] UpdateUserProfileRequest user)
    {
        if (!CanAccessUser(id))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        const string existsQuery = @"
            SELECT COUNT(1)
            FROM dbo.Users
            WHERE U_Id <> @U_Id AND (U_Email = @U_Email OR U_Username = @U_Username)";

        var duplicateCount = ExecuteScalar(existsQuery,
            new SqlParameter("@U_Id", id),
            new SqlParameter("@U_Email", user.U_Email),
            new SqlParameter("@U_Username", user.U_Username));

        if (duplicateCount > 0)
        {
            return Conflict(new { message = "Another user already has this email or username." });
        }

        const string query = @"
            UPDATE dbo.Users
            SET U_Name = @U_Name,
                U_Surname = @U_Surname,
                U_Email = @U_Email,
                U_Username = @U_Username,
                U_Phone = @U_Phone
            WHERE U_Id = @U_Id";

        var rows = ExecuteNonQuery(query,
            new SqlParameter("@U_Id", id),
            new SqlParameter("@U_Name", user.U_Name),
            new SqlParameter("@U_Surname", user.U_Surname),
            new SqlParameter("@U_Email", user.U_Email),
            new SqlParameter("@U_Username", user.U_Username),
            new SqlParameter("@U_Phone", string.IsNullOrWhiteSpace(user.U_Phone) ? DBNull.Value : user.U_Phone));

        return rows > 0 ? Ok(new { message = "User updated successfully." }) : NotFound();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public IActionResult DeleteUser(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.Equals(currentUserId, id.ToString(), StringComparison.Ordinal))
        {
            return BadRequest(new { message = "You cannot delete your own account." });
        }

        const string usageQuery = @"
            SELECT
                (SELECT COUNT(1) FROM dbo.Bookings WHERE U_Id = @U_Id) +
                (SELECT COUNT(1) FROM dbo.Contact_Form WHERE U_Id = @U_Id)";

        if (ExecuteScalar(usageQuery, new SqlParameter("@U_Id", id)) > 0)
        {
            return Conflict(new { message = "This user has bookings or contact messages and cannot be deleted." });
        }

        const string query = "DELETE FROM dbo.Users WHERE U_Id = @U_Id";
        var rows = ExecuteNonQuery(query, new SqlParameter("@U_Id", id));

        return rows > 0 ? Ok(new { message = "User deleted successfully." }) : NotFound();
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}/role")]
    public IActionResult UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.Equals(currentUserId, id.ToString(), StringComparison.Ordinal) && request.U_Type != "admin")
        {
            return BadRequest(new { message = "You cannot remove admin access from your own account." });
        }

        const string query = "UPDATE dbo.Users SET U_Type = @U_Type WHERE U_Id = @U_Id";
        var rows = ExecuteNonQuery(query,
            new SqlParameter("@U_Id", id),
            new SqlParameter("@U_Type", request.U_Type));

        return rows > 0 ? Ok(new { message = "User role updated successfully." }) : NotFound();
    }

    private bool CanAccessUser(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("admin");

        return isAdmin || string.Equals(currentUserId, id.ToString(), StringComparison.Ordinal);
    }

    private List<UserProfile> ExecuteUsersQuery(string query, params SqlParameter[] parameters)
    {
        var users = new List<UserProfile>();
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);

        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new UserProfile
            {
                U_Id = reader.GetInt32(reader.GetOrdinal("U_Id")),
                U_Name = reader.GetString(reader.GetOrdinal("U_Name")),
                U_Surname = reader.GetString(reader.GetOrdinal("U_Surname")),
                U_Email = reader.GetString(reader.GetOrdinal("U_Email")),
                U_Username = reader.GetString(reader.GetOrdinal("U_Username")),
                U_Phone = reader.IsDBNull(reader.GetOrdinal("U_Phone")) ? null : reader.GetString(reader.GetOrdinal("U_Phone")),
                U_Type = reader.GetString(reader.GetOrdinal("U_Type"))
            });
        }

        return users;
    }

    private int ExecuteNonQuery(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);

        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        connection.Open();
        return command.ExecuteNonQuery();
    }

    private int ExecuteScalar(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);

        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        connection.Open();
        var result = command.ExecuteScalar();

        return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }
}
