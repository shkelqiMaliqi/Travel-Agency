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

    [Authorize(Roles = "user,admin")]
    [HttpGet("mine")]
    public IActionResult GetMyBookings()
    {
        var userId = GetCurrentUserId();
        const string query = @"
            SELECT b.Booking_Id, b.Package_Id, b.U_Id,
                   (u.U_Name + ' ' + u.U_Surname) AS Customer_Name, u.U_Email, u.U_Phone,
                   tp.Package_Name, p.Place_Name, h.Hotel_Name,
                   b.Travelers, b.Total_Price, b.Booking_Status, b.Travel_Date, b.Booking_Date
            FROM dbo.Bookings b
            INNER JOIN dbo.Travel_Packages tp ON tp.Package_Id = b.Package_Id
            INNER JOIN dbo.Places p ON p.Place_Id = tp.Place_Id
            INNER JOIN dbo.Hotels h ON h.Hotel_Id = tp.Hotel_Id
            INNER JOIN dbo.Users u ON u.U_Id = b.U_Id
            WHERE b.U_Id = @U_Id
            ORDER BY b.Booking_Date DESC";

        return Ok(ExecuteBookingsQuery(query, new SqlParameter("@U_Id", userId)));
    }

    [Authorize(Roles = "admin,auditor")]
    [HttpGet]
    public IActionResult GetBookings()
    {
        const string query = @"
            SELECT b.Booking_Id, b.Package_Id, b.U_Id,
                   (u.U_Name + ' ' + u.U_Surname) AS Customer_Name, u.U_Email, u.U_Phone,
                   tp.Package_Name, p.Place_Name, h.Hotel_Name,
                   b.Travelers, b.Total_Price, b.Booking_Status, b.Travel_Date, b.Booking_Date
            FROM dbo.Bookings b
            INNER JOIN dbo.Travel_Packages tp ON tp.Package_Id = b.Package_Id
            INNER JOIN dbo.Places p ON p.Place_Id = tp.Place_Id
            INNER JOIN dbo.Hotels h ON h.Hotel_Id = tp.Hotel_Id
            INNER JOIN dbo.Users u ON u.U_Id = b.U_Id
            ORDER BY b.Booking_Date DESC";

        return Ok(ExecuteBookingsQuery(query));
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public IActionResult GetBooking(int id)
    {
        var userId = GetCurrentUserId();
        const string query = @"
            SELECT b.Booking_Id, b.Package_Id, b.U_Id,
                   (u.U_Name + ' ' + u.U_Surname) AS Customer_Name, u.U_Email, u.U_Phone,
                   tp.Package_Name, p.Place_Name, h.Hotel_Name,
                   b.Travelers, b.Total_Price, b.Booking_Status, b.Travel_Date, b.Booking_Date
            FROM dbo.Bookings b
            INNER JOIN dbo.Travel_Packages tp ON tp.Package_Id = b.Package_Id
            INNER JOIN dbo.Places p ON p.Place_Id = tp.Place_Id
            INNER JOIN dbo.Hotels h ON h.Hotel_Id = tp.Hotel_Id
            INNER JOIN dbo.Users u ON u.U_Id = b.U_Id
            WHERE b.Booking_Id = @Booking_Id
              AND (@CanViewAll = 1 OR b.U_Id = @U_Id)";

        var result = ExecuteBookingsQuery(query,
            new SqlParameter("@Booking_Id", id),
            new SqlParameter("@CanViewAll", CanViewAllBookings() ? 1 : 0),
            new SqlParameter("@U_Id", userId));

        return result.Count > 0 ? Ok(result[0]) : NotFound();
    }

    [Authorize(Roles = "user,admin")]
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
                SELECT Price_Per_Person, Available_Seats, Start_Date, End_Date
                FROM dbo.Travel_Packages WITH (UPDLOCK)
                WHERE Package_Id = @Package_Id";

            using var packageCommand = new SqlCommand(packageQuery, connection, transaction);
            packageCommand.Parameters.AddWithValue("@Package_Id", request.Package_Id);

            using var reader = packageCommand.ExecuteReader();
            if (!reader.Read())
            {
                reader.Close();
                transaction.Rollback();
                return NotFound(new { message = "Package not found." });
            }

            var pricePerPerson = reader.GetDecimal(reader.GetOrdinal("Price_Per_Person"));
            var availableSeats = reader.GetInt32(reader.GetOrdinal("Available_Seats"));
            var startDate = reader.GetDateTime(reader.GetOrdinal("Start_Date")).Date;
            var endDate = reader.GetDateTime(reader.GetOrdinal("End_Date")).Date;
            reader.Close();

            var travelDate = (request.Travel_Date ?? startDate).Date;
            if (travelDate < startDate || travelDate > endDate)
            {
                transaction.Rollback();
                return BadRequest(new { message = "Travel date must be between the package start and end dates." });
            }

            if (availableSeats < request.Travelers)
            {
                transaction.Rollback();
                return BadRequest(new { message = "Not enough seats are available for this package." });
            }

            const string updateSeatsQuery = @"
                UPDATE dbo.Travel_Packages
                SET Available_Seats = Available_Seats - @Travelers
                WHERE Package_Id = @Package_Id
                  AND Available_Seats >= @Travelers";

            var updatedSeats = ExecuteNonQuery(connection, transaction,
                updateSeatsQuery,
                new SqlParameter("@Package_Id", request.Package_Id),
                new SqlParameter("@Travelers", request.Travelers));

            if (updatedSeats == 0)
            {
                transaction.Rollback();
                return BadRequest(new { message = "Not enough seats are available for this package." });
            }

            var totalPrice = pricePerPerson * request.Travelers;
            const string insertQuery = @"
                INSERT INTO dbo.Bookings (Package_Id, U_Id, Travelers, Total_Price, Booking_Status, Travel_Date, Booking_Date)
                VALUES (@Package_Id, @U_Id, @Travelers, @Total_Price, 'Pending', @Travel_Date, GETUTCDATE())";

            ExecuteNonQuery(connection, transaction,
                insertQuery,
                new SqlParameter("@Package_Id", request.Package_Id),
                new SqlParameter("@U_Id", userId),
                new SqlParameter("@Travelers", request.Travelers),
                new SqlParameter("@Total_Price", totalPrice),
                new SqlParameter("@Travel_Date", travelDate));

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

        return ChangeBookingStatus(id, status, null);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPut("{id:int}/cancel")]
    public IActionResult CancelMyBooking(int id)
    {
        return ChangeBookingStatus(id, "Cancelled", GetCurrentUserId(), userCancellation: true);
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : 0;
    }

    private bool CanViewAllBookings()
    {
        return User.IsInRole("admin") || User.IsInRole("auditor");
    }

    private IActionResult ChangeBookingStatus(int bookingId, string nextStatus, int? ownerUserId, bool userCancellation = false)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("CRUDCS"));
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string bookingQuery = @"
                SELECT Booking_Id, Package_Id, U_Id, Travelers, Booking_Status
                FROM dbo.Bookings WITH (UPDLOCK)
                WHERE Booking_Id = @Booking_Id";

            using var bookingCommand = new SqlCommand(bookingQuery, connection, transaction);
            bookingCommand.Parameters.AddWithValue("@Booking_Id", bookingId);

            using var reader = bookingCommand.ExecuteReader();
            if (!reader.Read())
            {
                reader.Close();
                transaction.Rollback();
                return NotFound();
            }

            var packageId = reader.GetInt32(reader.GetOrdinal("Package_Id"));
            var userId = reader.GetInt32(reader.GetOrdinal("U_Id"));
            var travelers = reader.GetInt32(reader.GetOrdinal("Travelers"));
            var currentStatus = reader.GetString(reader.GetOrdinal("Booking_Status"));
            reader.Close();

            if (ownerUserId.HasValue && ownerUserId.Value != userId)
            {
                transaction.Rollback();
                return Forbid();
            }

            if (userCancellation && currentStatus != "Pending")
            {
                transaction.Rollback();
                return BadRequest(new { message = "Only pending bookings can be cancelled by the user." });
            }

            if (currentStatus == nextStatus)
            {
                transaction.Commit();
                return Ok(new { message = "Booking status already updated." });
            }

            if (IsCancelled(currentStatus) && !IsCancelled(nextStatus))
            {
                var updatedSeats = ExecuteNonQuery(connection, transaction,
                    @"UPDATE dbo.Travel_Packages
                      SET Available_Seats = Available_Seats - @Travelers
                      WHERE Package_Id = @Package_Id
                        AND Available_Seats >= @Travelers",
                    new SqlParameter("@Travelers", travelers),
                    new SqlParameter("@Package_Id", packageId));

                if (updatedSeats == 0)
                {
                    transaction.Rollback();
                    return BadRequest(new { message = "Not enough seats are available to reactivate this booking." });
                }
            }

            if (!IsCancelled(currentStatus) && IsCancelled(nextStatus))
            {
                ExecuteNonQuery(connection, transaction,
                    "UPDATE dbo.Travel_Packages SET Available_Seats = Available_Seats + @Travelers WHERE Package_Id = @Package_Id",
                    new SqlParameter("@Travelers", travelers),
                    new SqlParameter("@Package_Id", packageId));
            }

            ExecuteNonQuery(connection, transaction,
                "UPDATE dbo.Bookings SET Booking_Status = @Booking_Status WHERE Booking_Id = @Booking_Id",
                new SqlParameter("@Booking_Status", nextStatus),
                new SqlParameter("@Booking_Id", bookingId));

            transaction.Commit();
            return Ok(new { message = "Booking status updated successfully." });
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
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
                Customer_Name = reader.GetString(reader.GetOrdinal("Customer_Name")),
                Customer_Email = reader.GetString(reader.GetOrdinal("U_Email")),
                Customer_Phone = reader.IsDBNull(reader.GetOrdinal("U_Phone")) ? null : reader.GetString(reader.GetOrdinal("U_Phone")),
                Package_Name = reader.GetString(reader.GetOrdinal("Package_Name")),
                Place_Name = reader.GetString(reader.GetOrdinal("Place_Name")),
                Hotel_Name = reader.GetString(reader.GetOrdinal("Hotel_Name")),
                Travelers = reader.GetInt32(reader.GetOrdinal("Travelers")),
                Total_Price = reader.GetDecimal(reader.GetOrdinal("Total_Price")),
                Booking_Status = reader.GetString(reader.GetOrdinal("Booking_Status")),
                Travel_Date = reader.IsDBNull(reader.GetOrdinal("Travel_Date")) ? null : reader.GetDateTime(reader.GetOrdinal("Travel_Date")),
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

    private static int ExecuteNonQuery(SqlConnection connection, SqlTransaction transaction, string query, params SqlParameter[] parameters)
    {
        using var command = new SqlCommand(query, connection, transaction);
        command.Parameters.AddRange(parameters);

        return command.ExecuteNonQuery();
    }

    private static bool IsCancelled(string status)
    {
        return string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase);
    }
}
