using Travel_Agency_Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ContactController(ContactService service) : ApiControllerBase
{

    [AllowAnonymous]
    [HttpPost]
    [EnableRateLimiting("PublicWritePolicy")]
    public IActionResult CreateMessage([FromBody] ContactMessage message)
        => ToActionResult(service.CreateMessage(message));

    [Authorize(Roles = "admin,auditor")]
    [HttpGet]
    public IActionResult GetMessages()
        => ToActionResult(service.GetMessages());

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}/read")]
    public IActionResult MarkAsRead(int id)
        => ToActionResult(service.MarkAsRead(id));

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}/archive")]
    public IActionResult ArchiveMessage(int id)
        => ToActionResult(service.ArchiveMessage(id));

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}")]
    public IActionResult DeleteMessage(int id)
        => ToActionResult(service.DeleteMessage(id));

}
