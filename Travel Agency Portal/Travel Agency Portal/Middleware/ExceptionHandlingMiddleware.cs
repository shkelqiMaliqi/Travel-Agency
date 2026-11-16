using System.Net;
using System.Text.Json;
using Travel_Agency_Portal.Models;
using Travel_Agency_Portal.Services;

namespace Travel_Agency_Portal.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled API exception.");
            if (exception is not MfaDeliveryUnavailableException)
            {
                var alertingService = context.RequestServices.GetRequiredService<AlertingService>();
                await alertingService.NotifyAsync("Travel Agency API exception", exception.Message);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception is MfaDeliveryUnavailableException
                ? StatusCodes.Status503ServiceUnavailable : (int)HttpStatusCode.InternalServerError;

            var payload = ApiResponse<object>.Fail(exception is MfaDeliveryUnavailableException
                ? exception.Message : "An unexpected server error occurred.");
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}
