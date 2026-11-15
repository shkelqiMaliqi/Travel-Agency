using Microsoft.EntityFrameworkCore;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Data;

public sealed class InfrastructureRepository(TravelAgencyDbContext context)
{
    public Task<List<AuditLogEntry>> GetAuditLogsAsync() => context.AuditLogs.AsNoTracking()
        .OrderByDescending(x => x.Created_At).Take(100).ToListAsync();

    public Task<List<MetricsSnapshot>> GetMetricsSnapshotsAsync() => context.MetricsSnapshots.AsNoTracking()
        .OrderByDescending(x => x.Recorded_At).Take(50).ToListAsync();
}
