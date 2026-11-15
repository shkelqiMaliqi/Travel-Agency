using Travel_Agency_Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class BookingsController(BookingsService service) : ApiControllerBase
{

    [Authorize(Roles = "user,admin")]
    [HttpGet("mine")]
    public IActionResult GetMyBookings()
        => ToActionResult(service.GetMyBookings());

    [Authorize(Roles = "admin,auditor")]
    [HttpGet]
    public IActionResult GetBookings()
        => ToActionResult(service.GetBookings());

    [Authorize]
    [HttpGet("{id:int}")]
    public IActionResult GetBooking(int id)
        => ToActionResult(service.GetBooking(id));

    [Authorize(Roles = "user,admin")]
    [HttpPost]
    public IActionResult CreateBooking([FromBody] CreateBookingRequest request)
        => ToActionResult(service.CreateBooking(request));

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}/status")]
    public IActionResult UpdateBookingStatus(int id, [FromBody] string status)
        => ToActionResult(service.UpdateBookingStatus(id, status));

    [Authorize(Roles = "user,admin")]
    [HttpPut("{id:int}/cancel")]
    public IActionResult CancelMyBooking(int id)
        => ToActionResult(service.CancelMyBooking(id));

}
