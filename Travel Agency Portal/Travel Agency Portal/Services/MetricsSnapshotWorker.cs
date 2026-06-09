using Microsoft.Extensions.Hosting;
using Travel_Agency_Portal.Data;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public class MetricsSnapshotWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MetricsSnapshotWorker> _logger;

    public MetricsSnapshotWorker(IServiceScopeFactory scopeFactory, ILogger<MetricsSnapshotWorker> logger)
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
                var database = scope.ServiceProvider.GetRequiredService<DatabaseService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<TravelAgencyDbContext>();
                var auditService = scope.ServiceProvider.GetRequiredService<AuditLogService>();

                const string query = @"
                    SELECT
                        (SELECT COUNT(1) FROM dbo.Users) AS UsersCount,
                        (SELECT COUNT(1) FROM dbo.Bookings) AS BookingsCount,
                        (SELECT COUNT(1) FROM dbo.Travel_Packages) AS PackagesCount,
                        (SELECT COUNT(1) FROM dbo.Contact_Form WHERE C_IsRead = 0 AND C_IsArchived = 0) AS UnreadMessagesCount";

                var table = database.Query(query);
                if (table.Rows.Count > 0)
                {
                    var row = table.Rows[0];
                    dbContext.MetricsSnapshots.Add(new MetricsSnapshot
                    {
                        Users_Count = Convert.ToInt32(row["UsersCount"]),
                        Bookings_Count = Convert.ToInt32(row["BookingsCount"]),
                        Packages_Count = Convert.ToInt32(row["PackagesCount"]),
                        Unread_Messages_Count = Convert.ToInt32(row["UnreadMessagesCount"]),
                        Recorded_At = DateTime.UtcNow
                    });
                    await dbContext.SaveChangesAsync(stoppingToken);
                    await auditService.WriteSystemEventAsync("metrics.snapshot", "Background metrics snapshot recorded.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Metrics snapshot worker iteration failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
