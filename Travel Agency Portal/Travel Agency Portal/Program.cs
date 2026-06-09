using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using Travel_Agency_Portal.Data;
using Travel_Agency_Portal.Filters;
using Travel_Agency_Portal.Logging;
using Travel_Agency_Portal.Middleware;
using Travel_Agency_Portal.Models;
using Travel_Agency_Portal.Services;

var builder = WebApplication.CreateBuilder(args);

var logstashHost = builder.Configuration["Logging:Logstash:Host"];
if (!string.IsNullOrWhiteSpace(logstashHost))
{
    var logstashPort = builder.Configuration.GetValue("Logging:Logstash:Port", 5044);
    builder.Logging.AddProvider(new LogstashLoggerProvider(logstashHost, logstashPort, "travel-agency-api"));
}

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

        return new BadRequestObjectResult(ApiResponse<object>.Fail("Validation failed.", errors));
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Travel Agency API",
        Version = "v1",
        Description = "Travel Agency REST API with JWT authentication, MFA, infrastructure demos, and documented request/response examples."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.OperationFilter<SwaggerExamplesOperationFilter>();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:3001",
                "https://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHealthChecks();
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<TravelAgencyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CRUDCS")));

var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "travel-agency:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("AuthPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.AddPolicy("PublicWritePolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<MfaService>();
builder.Services.AddScoped<MfaDeliveryService>();
builder.Services.AddScoped<AlertingService>();
builder.Services.AddScoped<MongoAnalyticsService>();
builder.Services.AddScoped<S3StorageService>();
builder.Services.AddHostedService<CleanupWorker>();
builder.Services.AddHostedService<MetricsSnapshotWorker>();

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Missing Jwt:Secret configuration.");
var issuer = builder.Configuration["Jwt:Issuer"] ?? "TravelAgency.Api";
var audience = builder.Configuration["Jwt:Audience"] ?? "TravelAgency.Client";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "Travel Agency API Docs";
        options.DisplayRequestDuration();
        options.EnablePersistAuthorization();
    });
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseHttpMetrics();
app.UseResponseCaching();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapMetrics("/metrics");
app.MapControllers();

app.Run();

public partial class Program
{
}
