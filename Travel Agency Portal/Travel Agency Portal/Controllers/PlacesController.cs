using Travel_Agency_Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class PlacesController(PlacesService service) : ApiControllerBase
{

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetPlaces([FromQuery] string? search = null)
        => ToActionResult(service.GetPlaces(search));

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult GetPlace(int id)
        => ToActionResult(service.GetPlace(id));

    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult AddPlace([FromBody] Place place)
        => ToActionResult(service.AddPlace(place));

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}")]
    public IActionResult UpdatePlace(int id, [FromBody] Place place)
        => ToActionResult(service.UpdatePlace(id, place));

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public IActionResult DeletePlace(int id)
        => ToActionResult(service.DeletePlace(id));

}
