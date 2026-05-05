using System.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ContactController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ContactController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost]
    [EnableRateLimiting("PublicWritePolicy")]
    public IActionResult CreateMessage([FromBody] ContactMessage message)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        const string query = @"
            INSERT INTO dbo.Contact_Form (C_Name, C_Surname, C_Email, C_Subject, C_Message, U_Id, C_IsRead, C_IsArchived, C_CreatedAt)
            VALUES (@C_Name, @C_Surname, @C_Email, @C_Subject, @C_Message, @U_Id, 0, 0, SYSUTCDATETIME())";

        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        var userId = GetCurrentUserId();

        command.Parameters.AddWithValue("@C_Name", message.C_Name);
        command.Parameters.AddWithValue("@C_Surname", message.C_Surname);
        command.Parameters.AddWithValue("@C_Email", message.C_Email);
        command.Parameters.AddWithValue("@C_Subject", message.C_Subject);
        command.Parameters.AddWithValue("@C_Message", message.C_Message);
        command.Parameters.AddWithValue("@U_Id", userId.HasValue ? userId.Value : DBNull.Value);

        connection.Open();
        command.ExecuteNonQuery();

        return Ok(new { message = "Message sent successfully." });
    }

    [Authorize(Roles = "admin")]
    [HttpGet]
    public IActionResult GetMessages()
    {
        const string query = @"
            SELECT C_Id, C_Name, C_Surname, C_Email, C_Subject, C_Message, U_Id, C_IsRead, C_IsArchived, C_CreatedAt
            FROM dbo.Contact_Form
            WHERE C_IsArchived = 0
            ORDER BY C_CreatedAt DESC, C_Id DESC";

        var messages = new List<ContactMessage>();
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            messages.Add(new ContactMessage
            {
                C_Id = reader.GetInt32(reader.GetOrdinal("C_Id")),
                C_Name = reader.GetString(reader.GetOrdinal("C_Name")),
                C_Surname = reader.GetString(reader.GetOrdinal("C_Surname")),
                C_Email = reader.GetString(reader.GetOrdinal("C_Email")),
                C_Subject = reader.GetString(reader.GetOrdinal("C_Subject")),
                C_Message = reader.GetString(reader.GetOrdinal("C_Message")),
                U_Id = reader.IsDBNull(reader.GetOrdinal("U_Id")) ? null : reader.GetInt32(reader.GetOrdinal("U_Id")),
                C_IsRead = reader.GetBoolean(reader.GetOrdinal("C_IsRead")),
                C_IsArchived = reader.GetBoolean(reader.GetOrdinal("C_IsArchived")),
                C_CreatedAt = reader.GetDateTime(reader.GetOrdinal("C_CreatedAt"))
            });
        }

        return Ok(messages);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}/read")]
    public IActionResult MarkAsRead(int id)
    {
        const string query = "UPDATE dbo.Contact_Form SET C_IsRead = 1 WHERE C_Id = @C_Id";
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@C_Id", id);

        connection.Open();
        var rows = command.ExecuteNonQuery();

        return rows > 0 ? Ok(new { message = "Message marked as read." }) : NotFound();
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}/archive")]
    public IActionResult ArchiveMessage(int id)
    {
        const string query = "UPDATE dbo.Contact_Form SET C_IsArchived = 1 WHERE C_Id = @C_Id";
        var rows = ExecuteNonQuery(query, new SqlParameter("@C_Id", id));

        return rows > 0 ? Ok(new { message = "Message archived." }) : NotFound();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public IActionResult DeleteMessage(int id)
    {
        const string query = "DELETE FROM dbo.Contact_Form WHERE C_Id = @C_Id";
        var rows = ExecuteNonQuery(query, new SqlParameter("@C_Id", id));

        return rows > 0 ? Ok(new { message = "Message deleted." }) : NotFound();
    }

    private int ExecuteNonQuery(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
