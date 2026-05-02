using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class PackagesController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public PackagesController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetPackages()
    {
        const string query = @"
            SELECT tp.Package_Id, tp.Place_Id, tp.Hotel_Id, p.Place_Name, h.Hotel_Name,
                   tp.Package_Name, tp.Package_Description, tp.Price_Per_Person,
                   tp.Start_Date, tp.End_Date, tp.Available_Seats, tp.Package_Url
            FROM dbo.Travel_Packages tp
            INNER JOIN dbo.Places p ON p.Place_Id = tp.Place_Id
            INNER JOIN dbo.Hotels h ON h.Hotel_Id = tp.Hotel_Id
            ORDER BY tp.Start_Date ASC, tp.Package_Id DESC";

        return Ok(ExecutePackagesQuery(query));
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult AddPackage([FromBody] TravelPackage package)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (package.End_Date.Date < package.Start_Date.Date)
        {
            return BadRequest(new { message = "End date must be after start date." });
        }

        const string query = @"
            INSERT INTO dbo.Travel_Packages
                (Place_Id, Hotel_Id, Package_Name, Package_Description, Price_Per_Person, Start_Date, End_Date, Available_Seats, Package_Url)
            VALUES
                (@Place_Id, @Hotel_Id, @Package_Name, @Package_Description, @Price_Per_Person, @Start_Date, @End_Date, @Available_Seats, @Package_Url)";

        ExecuteNonQuery(query,
            new SqlParameter("@Place_Id", package.Place_Id),
            new SqlParameter("@Hotel_Id", package.Hotel_Id),
            new SqlParameter("@Package_Name", package.Package_Name),
            new SqlParameter("@Package_Description", package.Package_Description),
            new SqlParameter("@Price_Per_Person", package.Price_Per_Person),
            new SqlParameter("@Start_Date", package.Start_Date.Date),
            new SqlParameter("@End_Date", package.End_Date.Date),
            new SqlParameter("@Available_Seats", package.Available_Seats),
            new SqlParameter("@Package_Url", (object?)package.Package_Url ?? DBNull.Value));

        return Ok(new { message = "Package added successfully." });
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}")]
    public IActionResult UpdatePackage(int id, [FromBody] TravelPackage package)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (package.End_Date.Date < package.Start_Date.Date)
        {
            return BadRequest(new { message = "End date must be after start date." });
        }

        const string query = @"
            UPDATE dbo.Travel_Packages
            SET Place_Id = @Place_Id,
                Hotel_Id = @Hotel_Id,
                Package_Name = @Package_Name,
                Package_Description = @Package_Description,
                Price_Per_Person = @Price_Per_Person,
                Start_Date = @Start_Date,
                End_Date = @End_Date,
                Available_Seats = @Available_Seats,
                Package_Url = @Package_Url
            WHERE Package_Id = @Package_Id";

        var rows = ExecuteNonQuery(query,
            new SqlParameter("@Package_Id", id),
            new SqlParameter("@Place_Id", package.Place_Id),
            new SqlParameter("@Hotel_Id", package.Hotel_Id),
            new SqlParameter("@Package_Name", package.Package_Name),
            new SqlParameter("@Package_Description", package.Package_Description),
            new SqlParameter("@Price_Per_Person", package.Price_Per_Person),
            new SqlParameter("@Start_Date", package.Start_Date.Date),
            new SqlParameter("@End_Date", package.End_Date.Date),
            new SqlParameter("@Available_Seats", package.Available_Seats),
            new SqlParameter("@Package_Url", (object?)package.Package_Url ?? DBNull.Value));

        return rows > 0 ? Ok(new { message = "Package updated successfully." }) : NotFound();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public IActionResult DeletePackage(int id)
    {
        const string query = "DELETE FROM dbo.Travel_Packages WHERE Package_Id = @Package_Id";
        var rows = ExecuteNonQuery(query, new SqlParameter("@Package_Id", id));

        return rows > 0 ? Ok(new { message = "Package deleted successfully." }) : NotFound();
    }

    private List<TravelPackage> ExecutePackagesQuery(string query, params SqlParameter[] parameters)
    {
        var packages = new List<TravelPackage>();
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            packages.Add(new TravelPackage
            {
                Package_Id = reader.GetInt32(reader.GetOrdinal("Package_Id")),
                Place_Id = reader.GetInt32(reader.GetOrdinal("Place_Id")),
                Hotel_Id = reader.GetInt32(reader.GetOrdinal("Hotel_Id")),
                Place_Name = reader.GetString(reader.GetOrdinal("Place_Name")),
                Hotel_Name = reader.GetString(reader.GetOrdinal("Hotel_Name")),
                Package_Name = reader.GetString(reader.GetOrdinal("Package_Name")),
                Package_Description = reader.GetString(reader.GetOrdinal("Package_Description")),
                Price_Per_Person = reader.GetDecimal(reader.GetOrdinal("Price_Per_Person")),
                Start_Date = reader.GetDateTime(reader.GetOrdinal("Start_Date")),
                End_Date = reader.GetDateTime(reader.GetOrdinal("End_Date")),
                Available_Seats = reader.GetInt32(reader.GetOrdinal("Available_Seats")),
                Package_Url = reader.IsDBNull(reader.GetOrdinal("Package_Url")) ? null : reader.GetString(reader.GetOrdinal("Package_Url"))
            });
        }

        return packages;
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
