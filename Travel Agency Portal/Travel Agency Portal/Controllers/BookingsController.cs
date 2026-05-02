using System.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class BookingsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public BookingsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Authorize]
    [HttpGet("mine")]
    public IActionResult GetMyBookings()
    {
        var userId = GetCurrentUserId();
        const string query = @"
            SELECT b.Booking_Id, b.Package_Id, b.U_Id, tp.Package_Name, p.Place_Name, h.Hotel_Name,
                   b.Travelers, b.Total_Price, b.Booking_Status, b.Booking_Date
            FROM dbo.Bookings b
            INNER JOIN dbo.Travel_Packages tp ON tp.Package_Id = b.Package_Id
            INNER JOIN dbo.Places p ON p.Place_Id = tp.Place_Id
            INNER JOIN dbo.Hotels h ON h.Hotel_Id = tp.Hotel_Id
            WHERE b.U_Id = @U_Id
            ORDER BY b.Booking_Date DESC";

        return Ok(ExecuteBookingsQuery(query, new SqlParameter("@U_Id", userId)));
    }

    [Authorize(Roles = "admin")]
    [HttpGet]
    public IActionResult GetBookings()
    {
        const string query = @"
            SELECT b.Booking_Id, b.Package_Id, b.U_Id, tp.Package_Name, p.Place_Name, h.Hotel_Name,
                   b.Travelers, b.Total_Price, b.Booking_Status, b.Booking_Date
            FROM dbo.Bookings b
            INNER JOIN dbo.Travel_Packages tp ON tp.Package_Id = b.Package_Id
            INNER JOIN dbo.Places p ON p.Place_Id = tp.Place_Id
            INNER JOIN dbo.Hotels h ON h.Hotel_Id = tp.Hotel_Id
            ORDER BY b.Booking_Date DESC";

        return Ok(ExecuteBookingsQuery(query));
    }

    [Authorize]
    [HttpPost]
    public IActionResult CreateBooking([FromBody] CreateBookingRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetCurrentUserId();
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string packageQuery = @"
                SELECT Price_Per_Person, Available_Seats
                FROM dbo.Travel_Packages WITH (UPDLOCK)
                WHERE Package_Id = @Package_Id";

            using var packageCommand = new SqlCommand(packageQuery, connection, transaction);
            packageCommand.Parameters.AddWithValue("@Package_Id", request.Package_Id);

            using var reader = packageCommand.ExecuteReader();
            if (!reader.Read())
            {
                return NotFound(new { message = "Package not found." });
            }

            var pricePerPerson = reader.GetDecimal(reader.GetOrdinal("Price_Per_Person"));
            var availableSeats = reader.GetInt32(reader.GetOrdinal("Available_Seats"));
            reader.Close();

            if (availableSeats < request.Travelers)
            {
                return BadRequest(new { message = "Not enough seats are available for this package." });
            }

            var totalPrice = pricePerPerson * request.Travelers;
            const string insertQuery = @"
                INSERT INTO dbo.Bookings (Package_Id, U_Id, Travelers, Total_Price, Booking_Status, Booking_Date)
                VALUES (@Package_Id, @U_Id, @Travelers, @Total_Price, 'Pending', GETUTCDATE());

                UPDATE dbo.Travel_Packages
                SET Available_Seats = Available_Seats - @Travelers
                WHERE Package_Id = @Package_Id";

            using var insertCommand = new SqlCommand(insertQuery, connection, transaction);
            insertCommand.Parameters.AddWithValue("@Package_Id", request.Package_Id);
            insertCommand.Parameters.AddWithValue("@U_Id", userId);
            insertCommand.Parameters.AddWithValue("@Travelers", request.Travelers);
            insertCommand.Parameters.AddWithValue("@Total_Price", totalPrice);
            insertCommand.ExecuteNonQuery();

            transaction.Commit();
            return Ok(new { message = "Booking created successfully." });
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}/status")]
    public IActionResult UpdateBookingStatus(int id, [FromBody] string status)
    {
        var allowedStatuses = new[] { "Pending", "Confirmed", "Cancelled" };
        if (!allowedStatuses.Contains(status))
        {
            return BadRequest(new { message = "Status must be Pending, Confirmed, or Cancelled." });
        }

        const string query = "UPDATE dbo.Bookings SET Booking_Status = @Booking_Status WHERE Booking_Id = @Booking_Id";
        var rows = ExecuteNonQuery(query,
            new SqlParameter("@Booking_Id", id),
            new SqlParameter("@Booking_Status", status));

        return rows > 0 ? Ok(new { message = "Booking status updated successfully." }) : NotFound();
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : 0;
    }

    private List<Booking> ExecuteBookingsQuery(string query, params SqlParameter[] parameters)
    {
        var bookings = new List<Booking>();
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            bookings.Add(new Booking
            {
                Booking_Id = reader.GetInt32(reader.GetOrdinal("Booking_Id")),
                Package_Id = reader.GetInt32(reader.GetOrdinal("Package_Id")),
                U_Id = reader.GetInt32(reader.GetOrdinal("U_Id")),
                Package_Name = reader.GetString(reader.GetOrdinal("Package_Name")),
                Place_Name = reader.GetString(reader.GetOrdinal("Place_Name")),
                Hotel_Name = reader.GetString(reader.GetOrdinal("Hotel_Name")),
                Travelers = reader.GetInt32(reader.GetOrdinal("Travelers")),
                Total_Price = reader.GetDecimal(reader.GetOrdinal("Total_Price")),
                Booking_Status = reader.GetString(reader.GetOrdinal("Booking_Status")),
                Booking_Date = reader.GetDateTime(reader.GetOrdinal("Booking_Date"))
            });
        }

        return bookings;
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
