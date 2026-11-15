using Travel_Agency_Portal.Data;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public class InfrastructureService : OperationService
{
    private readonly InfrastructureRepository _repository;
    private readonly MongoAnalyticsService _mongoAnalyticsService;
    private readonly S3StorageService _s3StorageService;

    public InfrastructureService(
        InfrastructureRepository repository,
        MongoAnalyticsService mongoAnalyticsService,
        S3StorageService s3StorageService)
    {
        _repository = repository;
        _mongoAnalyticsService = mongoAnalyticsService;
        _s3StorageService = s3StorageService;
    }

    public async Task<OperationResult> GetAuditLogs()
    {
        var logs = await _repository.GetAuditLogsAsync();

        return Ok(logs);
    }

    public async Task<OperationResult> GetMetricsSnapshots()
    {
        var snapshots = await _repository.GetMetricsSnapshotsAsync();

        return Ok(snapshots);
    }

    public async Task<OperationResult> PushAnalyticsEvent(AnalyticsEventRequest request)
    {
        var stored = await _mongoAnalyticsService.TrackAsync(request.EventName, request.Category, request.Metadata);
        return Ok(new
        {
            stored,
            message = stored
                ? "Analytics event stored in MongoDB."
                : "MongoDB integration is not configured, so the event was skipped."
        });
    }

    public async Task<OperationResult> GetAnalyticsEvents()
    {
        var events = await _mongoAnalyticsService.GetRecentAsync();
        var result = events.Select(doc => doc.ToDictionary(
            pair => pair.Name,
            pair => pair.Value.ToString())).ToList();

        return Ok(result);
    }

    public async Task<OperationResult> UploadStorageDemo(StorageUploadRequest request)
    {
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
