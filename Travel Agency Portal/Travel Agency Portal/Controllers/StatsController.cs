using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Portal.Services;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class StatsController(StatsService service) : ApiControllerBase
{

    [Authorize(Roles = "admin,auditor")]
    [HttpGet("admin")]
    public IActionResult GetAdminStats()
        => ToActionResult(service.GetAdminStats());

}
