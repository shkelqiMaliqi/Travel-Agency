using Travel_Agency_Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class HotelsController(HotelsService service) : ApiControllerBase
{

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetHotels([FromQuery] int? placeId = null)
        => ToActionResult(service.GetHotels(placeId));

    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult AddHotel([FromBody] Hotel hotel)
        => ToActionResult(service.AddHotel(hotel));

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}")]
    public IActionResult UpdateHotel(int id, [FromBody] Hotel hotel)
        => ToActionResult(service.UpdateHotel(id, hotel));

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public IActionResult DeleteHotel(int id)
        => ToActionResult(service.DeleteHotel(id));

}
