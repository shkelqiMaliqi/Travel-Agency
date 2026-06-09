using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Travel_Agency_Portal.Filters;

public class SwaggerExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.ApiDescription.HttpMethod?.ToUpperInvariant();
        var path = ("/" + context.ApiDescription.RelativePath?.TrimStart('/')).ToLowerInvariant();

        AddStandardResponses(operation);

        if (method is null || path is null)
        {
            return;
        }

        switch ((method, path))
        {
            case ("POST", "/api/v1/auth/login"):
                operation.Summary = "Authenticate a user and return a JWT or an MFA challenge.";
                operation.Description = "Normal users receive a JWT immediately. Roles configured for MFA receive a temporary code and must call verify-mfa.";
                SetJsonRequestExample(operation, new OpenApiObject
                {
                    ["userNameOrEmail"] = new OpenApiString("admin@travelagency.com"),
                    ["password"] = new OpenApiString("Admin123!")
                });
                SetResponseExample(operation, "200", new OpenApiObject
                {
                    ["success"] = new OpenApiBoolean(true),
                    ["message"] = new OpenApiString("Request completed successfully."),
                    ["data"] = new OpenApiObject
                    {
                        ["requiresMfa"] = new OpenApiBoolean(true),
                        ["role"] = new OpenApiString("admin"),
                        ["userId"] = new OpenApiInteger(1),
                        ["name"] = new OpenApiString("Admin User"),
                        ["email"] = new OpenApiString("admin@travelagency.com"),
                        ["mfaCode"] = new OpenApiString("123456"),
                        ["mfaExpiresAtUtc"] = new OpenApiString("2026-06-09T22:15:00Z")
                    }
                });
                break;

            case ("POST", "/api/v1/auth/verify-mfa"):
                operation.Summary = "Verify a one-time MFA code and issue the final JWT.";
                SetJsonRequestExample(operation, new OpenApiObject
                {
                    ["email"] = new OpenApiString("admin@travelagency.com"),
                    ["code"] = new OpenApiString("123456")
                });
                SetResponseExample(operation, "200", new OpenApiObject
                {
                    ["success"] = new OpenApiBoolean(true),
                    ["message"] = new OpenApiString("Request completed successfully."),
                    ["data"] = new OpenApiObject
                    {
                        ["token"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.demo"),
                        ["expiresAtUtc"] = new OpenApiString("2026-06-09T23:15:00Z"),
                        ["role"] = new OpenApiString("admin"),
                        ["userId"] = new OpenApiInteger(1),
                        ["name"] = new OpenApiString("Admin User"),
                        ["email"] = new OpenApiString("admin@travelagency.com"),
                        ["requiresMfa"] = new OpenApiBoolean(false)
                    }
                });
                break;

            case ("GET", "/api/v1/places"):
                operation.Summary = "Return public destination data.";
                operation.Description = "Supports optional prefix search through the search query string parameter. Results are response-cached and can also be served from distributed cache.";
                SetResponseExample(operation, "200", new OpenApiObject
                {
                    ["success"] = new OpenApiBoolean(true),
                    ["message"] = new OpenApiString("Request completed successfully."),
                    ["data"] = new OpenApiArray
                    {
                        new OpenApiObject
                        {
                            ["place_Id"] = new OpenApiInteger(1),
                            ["place_Name"] = new OpenApiString("Paris"),
                            ["place_Description"] = new OpenApiString("City break destination."),
                            ["place_Url"] = new OpenApiString("https://example.com/paris.jpg")
                        }
                    }
                });
                break;

            case ("GET", "/api/v1/packages"):
                operation.Summary = "Return public travel packages with filtering support.";
                operation.Description = "Supports search, placeId, minPrice, and maxPrice filters. Results can be cached in Redis or in-memory distributed cache.";
                SetResponseExample(operation, "200", new OpenApiObject
                {
                    ["success"] = new OpenApiBoolean(true),
                    ["message"] = new OpenApiString("Request completed successfully."),
                    ["data"] = new OpenApiArray
                    {
                        new OpenApiObject
                        {
                            ["package_Id"] = new OpenApiInteger(1),
                            ["place_Id"] = new OpenApiInteger(1),
                            ["hotel_Id"] = new OpenApiInteger(1),
                            ["place_Name"] = new OpenApiString("Paris"),
                            ["hotel_Name"] = new OpenApiString("Hotel Lumiere"),
                            ["package_Name"] = new OpenApiString("Paris Spring Escape"),
                            ["price_Per_Person"] = new OpenApiDouble(899.99),
                            ["available_Seats"] = new OpenApiInteger(10)
                        }
                    }
                });
                break;

            case ("POST", "/api/v1/infrastructure/analytics-events"):
                operation.Summary = "Store a demo analytics event in MongoDB.";
                SetJsonRequestExample(operation, new OpenApiObject
                {
                    ["eventName"] = new OpenApiString("booking.completed"),
                    ["category"] = new OpenApiString("analytics"),
                    ["metadata"] = new OpenApiString("{\"packageId\":1,\"source\":\"swagger\"}")
                });
                SetResponseExample(operation, "200", new OpenApiObject
                {
                    ["success"] = new OpenApiBoolean(true),
                    ["message"] = new OpenApiString("Request completed successfully."),
                    ["data"] = new OpenApiObject
                    {
                        ["stored"] = new OpenApiBoolean(true),
                        ["message"] = new OpenApiString("Analytics event stored in MongoDB.")
                    }
                });
                break;

            case ("POST", "/api/v1/infrastructure/storage-demo"):
                operation.Summary = "Upload a text file to S3-compatible storage.";
                SetJsonRequestExample(operation, new OpenApiObject
                {
                    ["fileName"] = new OpenApiString("demo.txt"),
                    ["content"] = new OpenApiString("Travel Agency storage integration demo.")
                });
                SetResponseExample(operation, "200", new OpenApiObject
                {
                    ["success"] = new OpenApiBoolean(true),
                    ["message"] = new OpenApiString("Request completed successfully."),
                    ["data"] = new OpenApiObject
                    {
                        ["uploaded"] = new OpenApiBoolean(true),
                        ["message"] = new OpenApiString("File uploaded to S3-compatible storage.")
                    }
                });
                break;
        }
    }

    private static void AddStandardResponses(OpenApiOperation operation)
    {
        AddResponseIfMissing(operation, "400", "Bad request or validation failure.", "Validation failed.");
        AddResponseIfMissing(operation, "401", "Authentication required or credentials invalid.", "Unauthorized.");
        AddResponseIfMissing(operation, "403", "Authenticated user does not have enough permissions.", "Forbidden.");
        AddResponseIfMissing(operation, "429", "Too many requests for the configured rate limit window.", "Too many requests.");
        AddResponseIfMissing(operation, "500", "Unexpected server error handled by middleware.", "An unexpected error occurred.");
    }

    private static void AddResponseIfMissing(OpenApiOperation operation, string statusCode, string description, string message)
    {
        if (operation.Responses.ContainsKey(statusCode))
        {
            return;
        }

        operation.Responses[statusCode] = new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new()
                {
                    Example = new OpenApiObject
                    {
                        ["success"] = new OpenApiBoolean(false),
                        ["message"] = new OpenApiString(message),
                        ["data"] = new OpenApiObject()
                    }
                }
            }
        };
    }

    private static void SetJsonRequestExample(OpenApiOperation operation, IOpenApiAny example)
    {
        if (operation.RequestBody?.Content.TryGetValue("application/json", out var content) == true)
        {
            content.Example = example;
        }
    }

    private static void SetResponseExample(OpenApiOperation operation, string statusCode, IOpenApiAny example)
    {
        if (!operation.Responses.TryGetValue(statusCode, out var response))
        {
            response = new OpenApiResponse { Description = "Successful response." };
            operation.Responses[statusCode] = response;
        }

        response.Content ??= new Dictionary<string, OpenApiMediaType>();
        response.Content["application/json"] = new OpenApiMediaType
        {
            Example = example
        };
    }
}
