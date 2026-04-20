using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public IActionResult CreateMessage([FromBody] ContactMessage message)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        const string query = @"
            INSERT INTO dbo.Contact_Form (C_Name, C_Surname, C_Email, C_Subject, C_Message, U_Id)
            VALUES (@C_Name, @C_Surname, @C_Email, @C_Subject, @C_Message, @U_Id)";

        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@C_Name", message.C_Name);
        command.Parameters.AddWithValue("@C_Surname", message.C_Surname);
        command.Parameters.AddWithValue("@C_Email", message.C_Email);
        command.Parameters.AddWithValue("@C_Subject", message.C_Subject);
        command.Parameters.AddWithValue("@C_Message", message.C_Message);
        command.Parameters.AddWithValue("@U_Id", (object?)message.U_Id ?? DBNull.Value);

        connection.Open();
        command.ExecuteNonQuery();

        return Ok(new { message = "Message sent successfully." });
    }
}
