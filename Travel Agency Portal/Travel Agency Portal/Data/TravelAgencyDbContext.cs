using Microsoft.EntityFrameworkCore;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Data;

public class TravelAgencyDbContext : DbContext
{
    public TravelAgencyDbContext(DbContextOptions<TravelAgencyDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();
    public DbSet<UserMfaCode> UserMfaCodes => Set<UserMfaCode>();
    public DbSet<MetricsSnapshot> MetricsSnapshots => Set<MetricsSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogEntry>().ToTable("Audit_Logs").HasKey(x => x.Audit_Id);
        modelBuilder.Entity<UserMfaCode>().ToTable("User_Mfa_Codes").HasKey(x => x.Mfa_Id);
        modelBuilder.Entity<MetricsSnapshot>().ToTable("Metrics_Snapshots").HasKey(x => x.Snapshot_Id);
    }
}
