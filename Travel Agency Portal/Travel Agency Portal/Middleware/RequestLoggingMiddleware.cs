using Travel_Agency_Portal.Services;

namespace Travel_Agency_Portal.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startedAt = DateTime.UtcNow;

        await _next(context);

        var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;
        _logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMs);

        var shouldAudit = context.User.Identity?.IsAuthenticated == true ||
                          context.Request.Path.StartsWithSegments("/api/v1/auth") ||
                          context.Request.Path.StartsWithSegments("/api/v1/contact");

        if (shouldAudit)
        {
            var auditLogService = context.RequestServices.GetRequiredService<AuditLogService>();
            await auditLogService.WriteAsync(context, "http.request", $"ElapsedMs={elapsedMs:F2}");
        }
    }
}
