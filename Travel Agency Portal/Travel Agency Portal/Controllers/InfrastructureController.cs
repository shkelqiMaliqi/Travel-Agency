using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Travel_Agency_Portal.Data;
using Travel_Agency_Portal.Models;
using Travel_Agency_Portal.Services;

namespace Travel_Agency_Portal.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize(Roles = "admin")]
public class InfrastructureController : ControllerBase
{
    private readonly TravelAgencyDbContext _dbContext;
    private readonly MongoAnalyticsService _mongoAnalyticsService;
    private readonly S3StorageService _s3StorageService;

    public InfrastructureController(
        TravelAgencyDbContext dbContext,
        MongoAnalyticsService mongoAnalyticsService,
        S3StorageService s3StorageService)
    {
        _dbContext = dbContext;
        _mongoAnalyticsService = mongoAnalyticsService;
        _s3StorageService = s3StorageService;
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs()
    {
        var logs = await _dbContext.AuditLogs
            .OrderByDescending(x => x.Created_At)
            .Take(100)
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("metrics-snapshots")]
    public async Task<IActionResult> GetMetricsSnapshots()
    {
        var snapshots = await _dbContext.MetricsSnapshots
            .OrderByDescending(x => x.Recorded_At)
            .Take(50)
            .ToListAsync();

        return Ok(snapshots);
    }

    [HttpPost("analytics-events")]
    public async Task<IActionResult> PushAnalyticsEvent([FromBody] AnalyticsEventRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var stored = await _mongoAnalyticsService.TrackAsync(request.EventName, request.Category, request.Metadata);
        return Ok(new
        {
            stored,
            message = stored
                ? "Analytics event stored in MongoDB."
                : "MongoDB integration is not configured, so the event was skipped."
        });
    }

    [HttpGet("analytics-events")]
    public async Task<IActionResult> GetAnalyticsEvents()
    {
        var events = await _mongoAnalyticsService.GetRecentAsync();
        var result = events.Select(doc => doc.ToDictionary(
            pair => pair.Name,
            pair => pair.Value.ToString())).ToList();

        return Ok(result);
    }

    [HttpPost("storage-demo")]
    public async Task<IActionResult> UploadStorageDemo([FromBody] StorageUploadRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var uploaded = await _s3StorageService.UploadTextAsync(request.FileName, request.Content);
        return Ok(new
        {
            uploaded,
            message = uploaded
                ? "File uploaded to S3-compatible storage."
                : "S3-compatible storage is not configured, so the upload was skipped."
        });
    }
}
