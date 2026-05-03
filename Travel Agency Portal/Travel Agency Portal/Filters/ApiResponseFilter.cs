using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Travel_Agency_Portal.Models;

namespace Travel_Agency_Portal.Filters;

public class ApiResponseFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is StatusCodeResult statusCodeResult && statusCodeResult.StatusCode >= 400)
        {
            context.Result = new ObjectResult(ApiResponse<object>.Fail(MessageForStatus(statusCodeResult.StatusCode)))
            {
                StatusCode = statusCodeResult.StatusCode
            };
            return;
        }

        if (context.Result is not ObjectResult objectResult)
        {
            return;
        }

        var statusCode = objectResult.StatusCode ?? context.HttpContext.Response.StatusCode;
        if (IsApiResponse(objectResult.Value))
        {
            return;
        }

        if (statusCode < 200 || statusCode >= 300)
        {
            objectResult.Value = ApiResponse<object>.Fail(ExtractMessage(objectResult.Value), objectResult.Value);
            return;
        }

        if (objectResult.Value is ProblemDetails)
        {
            return;
        }

        objectResult.Value = ApiResponse<object>.Ok(objectResult.Value);
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static bool IsApiResponse(object? value)
    {
        return value?.GetType().IsGenericType == true &&
               value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>);
    }

    private static string ExtractMessage(object? value)
    {
        if (value is null)
        {
            return "Request failed.";
        }

        var messageProperty =
            value.GetType().GetProperty("message") ??
            value.GetType().GetProperty("Message") ??
            value.GetType().GetProperty("title") ??
            value.GetType().GetProperty("Title");
        var message = messageProperty?.GetValue(value)?.ToString();

        return string.IsNullOrWhiteSpace(message) ? "Request failed." : message;
    }

    private static string MessageForStatus(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request.",
            StatusCodes.Status401Unauthorized => "Unauthorized.",
            StatusCodes.Status403Forbidden => "Forbidden.",
            StatusCodes.Status404NotFound => "Resource not found.",
            StatusCodes.Status409Conflict => "Conflict.",
            _ => "Request failed."
        };
    }
}
