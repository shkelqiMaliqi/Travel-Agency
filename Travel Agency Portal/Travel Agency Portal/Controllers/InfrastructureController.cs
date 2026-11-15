using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Travel_Agency_Portal.Models;
using Travel_Agency_Portal.Services;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize(Roles = "admin,auditor")]
public class InfrastructureController(InfrastructureService service) : ApiControllerBase
{

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs()
        => ToActionResult(await service.GetAuditLogs());

    [HttpGet("metrics-snapshots")]
    public async Task<IActionResult> GetMetricsSnapshots()
        => ToActionResult(await service.GetMetricsSnapshots());

    [HttpPost("analytics-events")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> PushAnalyticsEvent([FromBody] AnalyticsEventRequest request)
        => ToActionResult(await service.PushAnalyticsEvent(request));

    [HttpGet("analytics-events")]
    public async Task<IActionResult> GetAnalyticsEvents()
        => ToActionResult(await service.GetAnalyticsEvents());

    [HttpPost("storage-demo")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UploadStorageDemo([FromBody] StorageUploadRequest request)
        => ToActionResult(await service.UploadStorageDemo(request));

}
