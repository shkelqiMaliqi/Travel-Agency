using System.Data;
using System.Data.SqlClient;
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
        return new JsonResult(ExecuteQuery(query));
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public IActionResult GetUser(int id)
    {
        const string query = @"SELECT U_Id, U_Name, U_Surname, U_Email, U_Username, U_Phone, U_Type FROM dbo.Users WHERE U_Id = @U_Id";
        var parameters = new[] { new SqlParameter("@U_Id", id) };
        var result = ExecuteQuery(query, parameters);

        return result.Rows.Count > 0 ? Ok(result.Rows[0]) : NotFound();
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public IActionResult UpdateUser(int id, [FromBody] Users user)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        const string query = @"
            UPDATE dbo.Users
            SET U_Name = @U_Name,
                U_Surname = @U_Surname,
                U_Email = @U_Email,
                U_Username = @U_Username,
                U_Phone = @U_Phone,
                U_Type = @U_Type
            WHERE U_Id = @U_Id";

        var rows = ExecuteNonQuery(query,
            new SqlParameter("@U_Id", id),
            new SqlParameter("@U_Name", user.U_Name),
            new SqlParameter("@U_Surname", user.U_Surname),
            new SqlParameter("@U_Email", user.U_Email),
            new SqlParameter("@U_Username", user.U_Username),
            new SqlParameter("@U_Phone", (object?)user.U_Phone ?? DBNull.Value),
            new SqlParameter("@U_Type", user.U_Type));

        return rows > 0 ? Ok(new { message = "User updated successfully." }) : NotFound();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public IActionResult DeleteUser(int id)
    {
        const string query = "DELETE FROM dbo.Users WHERE U_Id = @U_Id";
        var rows = ExecuteNonQuery(query, new SqlParameter("@U_Id", id));

        return rows > 0 ? Ok(new { message = "User deleted successfully." }) : NotFound();
    }

    private DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
    {
        var table = new DataTable();
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);

        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        connection.Open();
        using var adapter = new SqlDataAdapter(command);
        adapter.Fill(table);

        return table;
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
}
