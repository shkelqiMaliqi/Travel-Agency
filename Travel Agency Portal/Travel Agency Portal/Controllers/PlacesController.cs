using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class PlacesController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public PlacesController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetPlaces()
    {
        const string query = "SELECT Place_Id, Place_Name, Place_Description, Place_Url FROM dbo.Places ORDER BY Place_Id DESC";
        return new JsonResult(ExecuteQuery(query));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult GetPlace(int id)
    {
        const string query = "SELECT Place_Id, Place_Name, Place_Description, Place_Url FROM dbo.Places WHERE Place_Id = @Place_Id";
        var result = ExecuteQuery(query, new SqlParameter("@Place_Id", id));

        return result.Rows.Count > 0 ? Ok(result.Rows[0]) : NotFound();
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult AddPlace([FromBody] Place place)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        const string query = @"
            INSERT INTO dbo.Places (Place_Name, Place_Description, Place_Url)
            VALUES (@Place_Name, @Place_Description, @Place_Url)";

        ExecuteNonQuery(query,
            new SqlParameter("@Place_Name", place.Place_Name),
            new SqlParameter("@Place_Description", place.Place_Description),
            new SqlParameter("@Place_Url", (object?)place.Place_Url ?? DBNull.Value));

        return Ok(new { message = "Place added successfully." });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}")]
    public IActionResult UpdatePlace(int id, [FromBody] Place place)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        const string query = @"
            UPDATE dbo.Places
            SET Place_Name = @Place_Name,
                Place_Description = @Place_Description,
                Place_Url = @Place_Url
            WHERE Place_Id = @Place_Id";

        var rows = ExecuteNonQuery(query,
            new SqlParameter("@Place_Id", id),
            new SqlParameter("@Place_Name", place.Place_Name),
            new SqlParameter("@Place_Description", place.Place_Description),
            new SqlParameter("@Place_Url", (object?)place.Place_Url ?? DBNull.Value));

        return rows > 0 ? Ok(new { message = "Place updated successfully." }) : NotFound();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public IActionResult DeletePlace(int id)
    {
        const string query = "DELETE FROM dbo.Places WHERE Place_Id = @Place_Id";
        var rows = ExecuteNonQuery(query, new SqlParameter("@Place_Id", id));

        return rows > 0 ? Ok(new { message = "Place deleted successfully." }) : NotFound();
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
