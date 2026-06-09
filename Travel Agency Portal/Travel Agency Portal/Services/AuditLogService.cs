using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Travel_Agency_Portal.Data;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Services;

public class AuditLogService
{
    private readonly TravelAgencyDbContext _dbContext;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(TravelAgencyDbContext dbContext, ILogger<AuditLogService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task WriteAsync(HttpContext context, string eventType, string? details = null)
    {
        try
        {
            var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = context.User.FindFirstValue(ClaimTypes.Email);

            var entry = new AuditLogEntry
            {
                Event_Type = eventType,
                User_Email = email,
                U_Id = int.TryParse(userIdValue, out var userId) ? userId : null,
                Request_Path = context.Request.Path,
                Http_Method = context.Request.Method,
                Status_Code = context.Response.StatusCode,
                Details = details,
                Created_At = DateTime.UtcNow
            };

            _dbContext.AuditLogs.Add(entry);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist audit log entry.");
        }
    }

    public async Task WriteSystemEventAsync(string eventType, string details)
    {
        try
        {
            _dbContext.AuditLogs.Add(new AuditLogEntry
            {
                Event_Type = eventType,
                Request_Path = "system",
                Http_Method = "SYSTEM",
                Status_Code = 200,
                Details = details,
                Created_At = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist system audit log entry.");
        }
    }
}
