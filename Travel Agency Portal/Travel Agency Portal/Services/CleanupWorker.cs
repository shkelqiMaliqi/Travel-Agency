using Microsoft.EntityFrameworkCore;
using Travel_Agency_Portal.Data;

namespace Travel_Agency_Portal.Services;

public class CleanupWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CleanupWorker> _logger;

    public CleanupWorker(IServiceScopeFactory scopeFactory, ILogger<CleanupWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TravelAgencyDbContext>();
                var auditService = scope.ServiceProvider.GetRequiredService<AuditLogService>();

                await dbContext.UserMfaCodes
                    .Where(x => x.Expires_At < DateTime.UtcNow || x.Is_Used)
                    .ExecuteDeleteAsync(stoppingToken);

                await auditService.WriteSystemEventAsync("maintenance.cleanup", "Expired MFA codes cleaned up.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cleanup worker iteration failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
