using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Portal.Services;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class StatsController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public StatsController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [Authorize(Roles = "admin")]
    [HttpGet("admin")]
    public IActionResult GetAdminStats()
    {
        const string query = @"
            SELECT
                (SELECT COUNT(1) FROM dbo.Users) AS UsersCount,
                (SELECT COUNT(1) FROM dbo.Bookings WHERE Booking_Status <> 'Cancelled') AS BookingsCount,
                (SELECT COUNT(1) FROM dbo.Bookings WHERE Booking_Status = 'Pending') AS PendingBookingsCount,
                (SELECT COUNT(1) FROM dbo.Travel_Packages) AS PackagesCount,
                (SELECT COUNT(1) FROM dbo.Travel_Packages WHERE Available_Seats = 0) AS SoldOutPackagesCount,
                (SELECT ISNULL(SUM(Total_Price), 0) FROM dbo.Bookings WHERE Booking_Status <> 'Cancelled') AS Revenue";

        var table = _databaseService.Query(query);
        if (table.Rows.Count == 0)
        {
            return Ok(new { });
        }

        var row = table.Rows[0];
        return Ok(new
        {
            usersCount = Convert.ToInt32(row["UsersCount"]),
            bookingsCount = Convert.ToInt32(row["BookingsCount"]),
            pendingBookingsCount = Convert.ToInt32(row["PendingBookingsCount"]),
            packagesCount = Convert.ToInt32(row["PackagesCount"]),
            soldOutPackagesCount = Convert.ToInt32(row["SoldOutPackagesCount"]),
            revenue = Convert.ToDecimal(row["Revenue"])
        });
    }
}
