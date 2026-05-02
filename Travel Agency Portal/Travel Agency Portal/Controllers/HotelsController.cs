using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class HotelsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public HotelsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetHotels([FromQuery] int? placeId = null)
    {
        var query = @"
            SELECT h.Hotel_Id, h.Place_Id, p.Place_Name, h.Hotel_Name, h.Hotel_Description, h.Hotel_Stars, h.Hotel_Url
            FROM dbo.Hotels h
            INNER JOIN dbo.Places p ON p.Place_Id = h.Place_Id";

        var parameters = new List<SqlParameter>();
        if (placeId.HasValue)
        {
            query += " WHERE h.Place_Id = @Place_Id";
            parameters.Add(new SqlParameter("@Place_Id", placeId.Value));
        }

        query += " ORDER BY h.Hotel_Id DESC";
        return Ok(ExecuteHotelsQuery(query, parameters.ToArray()));
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult AddHotel([FromBody] Hotel hotel)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        const string query = @"
            INSERT INTO dbo.Hotels (Place_Id, Hotel_Name, Hotel_Description, Hotel_Stars, Hotel_Url)
            VALUES (@Place_Id, @Hotel_Name, @Hotel_Description, @Hotel_Stars, @Hotel_Url)";

        ExecuteNonQuery(query,
            new SqlParameter("@Place_Id", hotel.Place_Id),
            new SqlParameter("@Hotel_Name", hotel.Hotel_Name),
            new SqlParameter("@Hotel_Description", hotel.Hotel_Description),
            new SqlParameter("@Hotel_Stars", hotel.Hotel_Stars),
            new SqlParameter("@Hotel_Url", (object?)hotel.Hotel_Url ?? DBNull.Value));

        return Ok(new { message = "Hotel added successfully." });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}")]
    public IActionResult UpdateHotel(int id, [FromBody] Hotel hotel)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        const string query = @"
            UPDATE dbo.Hotels
            SET Place_Id = @Place_Id,
                Hotel_Name = @Hotel_Name,
                Hotel_Description = @Hotel_Description,
                Hotel_Stars = @Hotel_Stars,
                Hotel_Url = @Hotel_Url
            WHERE Hotel_Id = @Hotel_Id";

        var rows = ExecuteNonQuery(query,
            new SqlParameter("@Hotel_Id", id),
            new SqlParameter("@Place_Id", hotel.Place_Id),
            new SqlParameter("@Hotel_Name", hotel.Hotel_Name),
            new SqlParameter("@Hotel_Description", hotel.Hotel_Description),
            new SqlParameter("@Hotel_Stars", hotel.Hotel_Stars),
            new SqlParameter("@Hotel_Url", (object?)hotel.Hotel_Url ?? DBNull.Value));

        return rows > 0 ? Ok(new { message = "Hotel updated successfully." }) : NotFound();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public IActionResult DeleteHotel(int id)
    {
        const string query = "DELETE FROM dbo.Hotels WHERE Hotel_Id = @Hotel_Id";
        var rows = ExecuteNonQuery(query, new SqlParameter("@Hotel_Id", id));

        return rows > 0 ? Ok(new { message = "Hotel deleted successfully." }) : NotFound();
    }

    private List<Hotel> ExecuteHotelsQuery(string query, params SqlParameter[] parameters)
    {
        var hotels = new List<Hotel>();
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            hotels.Add(new Hotel
            {
                Hotel_Id = reader.GetInt32(reader.GetOrdinal("Hotel_Id")),
                Place_Id = reader.GetInt32(reader.GetOrdinal("Place_Id")),
                Place_Name = reader.GetString(reader.GetOrdinal("Place_Name")),
                Hotel_Name = reader.GetString(reader.GetOrdinal("Hotel_Name")),
                Hotel_Description = reader.GetString(reader.GetOrdinal("Hotel_Description")),
                Hotel_Stars = reader.GetInt32(reader.GetOrdinal("Hotel_Stars")),
                Hotel_Url = reader.IsDBNull(reader.GetOrdinal("Hotel_Url")) ? null : reader.GetString(reader.GetOrdinal("Hotel_Url"))
            });
        }

        return hotels;
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
