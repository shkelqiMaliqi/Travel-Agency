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
    public IActionResult GetPlaces([FromQuery] string? search = null)
    {
        var query = "SELECT Place_Id, Place_Name, Place_Description, Place_Url FROM dbo.Places";
        var parameters = new List<SqlParameter>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query += " WHERE Place_Name LIKE @Search OR Place_Description LIKE @Search";
            parameters.Add(new SqlParameter("@Search", $"%{search.Trim()}%"));
        }

        query += " ORDER BY Place_Id DESC";
        return Ok(ExecutePlacesQuery(query, parameters.ToArray()));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult GetPlace(int id)
    {
        const string query = "SELECT Place_Id, Place_Name, Place_Description, Place_Url FROM dbo.Places WHERE Place_Id = @Place_Id";
        var result = ExecutePlacesQuery(query, new SqlParameter("@Place_Id", id));

        return result.Count > 0 ? Ok(result[0]) : NotFound();
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
        const string usageQuery = @"
            SELECT
                (SELECT COUNT(1) FROM dbo.Hotels WHERE Place_Id = @Place_Id) +
                (SELECT COUNT(1) FROM dbo.Travel_Packages WHERE Place_Id = @Place_Id)";

        if (ExecuteScalar(usageQuery, new SqlParameter("@Place_Id", id)) > 0)
        {
            return Conflict(new { message = "This destination has hotels or packages. Delete those first." });
        }

        const string query = "DELETE FROM dbo.Places WHERE Place_Id = @Place_Id";
        var rows = ExecuteNonQuery(query, new SqlParameter("@Place_Id", id));

        return rows > 0 ? Ok(new { message = "Place deleted successfully." }) : NotFound();
    }

    private List<Place> ExecutePlacesQuery(string query, params SqlParameter[] parameters)
    {
        var places = new List<Place>();
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
            places.Add(new Place
            {
                Place_Id = reader.GetInt32(reader.GetOrdinal("Place_Id")),
                Place_Name = reader.GetString(reader.GetOrdinal("Place_Name")),
                Place_Description = reader.GetString(reader.GetOrdinal("Place_Description")),
                Place_Url = reader.IsDBNull(reader.GetOrdinal("Place_Url"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Place_Url"))
            });
        }

        return places;
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
