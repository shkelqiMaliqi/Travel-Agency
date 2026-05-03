using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class StatsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public StatsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Authorize(Roles = "admin")]
    [HttpGet("admin")]
    public IActionResult GetAdminStats()
    {
        const string query = @"
            SELECT
                (SELECT COUNT(1) FROM dbo.Users) AS UsersCount,
                (SELECT COUNT(1) FROM dbo.Bookings) AS BookingsCount,
                (SELECT COUNT(1) FROM dbo.Bookings WHERE Booking_Status = 'Pending') AS PendingBookingsCount,
                (SELECT COUNT(1) FROM dbo.Travel_Packages) AS PackagesCount,
                (SELECT COUNT(1) FROM dbo.Travel_Packages WHERE Available_Seats = 0) AS SoldOutPackagesCount,
                (SELECT ISNULL(SUM(Total_Price), 0) FROM dbo.Bookings WHERE Booking_Status <> 'Cancelled') AS Revenue";

        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);

        connection.Open();
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return Ok(new { });
        }

        return Ok(new
        {
            usersCount = reader.GetInt32(reader.GetOrdinal("UsersCount")),
            bookingsCount = reader.GetInt32(reader.GetOrdinal("BookingsCount")),
            pendingBookingsCount = reader.GetInt32(reader.GetOrdinal("PendingBookingsCount")),
            packagesCount = reader.GetInt32(reader.GetOrdinal("PackagesCount")),
            soldOutPackagesCount = reader.GetInt32(reader.GetOrdinal("SoldOutPackagesCount")),
            revenue = reader.GetDecimal(reader.GetOrdinal("Revenue"))
        });
    }
}
