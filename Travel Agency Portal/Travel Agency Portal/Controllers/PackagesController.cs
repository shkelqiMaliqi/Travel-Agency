using Travel_Agency_Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Portal.Dtos;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class PackagesController(PackagesService service) : ApiControllerBase
{

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetPackages([FromQuery] string? search = null, [FromQuery] int? placeId = null, [FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null)
        => ToActionResult(service.GetPackages(search, placeId, minPrice, maxPrice));

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult GetPackage(int id)
        => ToActionResult(service.GetPackage(id));

    [Authorize(Roles = "admin")]
    [HttpPost]
    public IActionResult AddPackage([FromBody] PackageRequestDto package)
        => ToActionResult(service.AddPackage(package));

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}")]
    public IActionResult UpdatePackage(int id, [FromBody] PackageRequestDto package)
        => ToActionResult(service.UpdatePackage(id, package));

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public IActionResult DeletePackage(int id)
        => ToActionResult(service.DeletePackage(id));

}
