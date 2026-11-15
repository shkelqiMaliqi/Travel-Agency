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
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes
        .Concat(new[] { "application/json", "application/javascript" });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(provider =>
{
    var user = provider.GetRequiredService<IHttpContextAccessor>().HttpContext?.User;
    var id = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    return new RequestActor(int.TryParse(id, out var userId) ? userId : null,
        (user?.FindAll(System.Security.Claims.ClaimTypes.Role).Select(claim => claim.Value)
            ?? Enumerable.Empty<string>()).ToHashSet(StringComparer.Ordinal));
});
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthRepository>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<UsersRepository>();
builder.Services.AddScoped<BookingsService>();
builder.Services.AddScoped<BookingsRepository>();
builder.Services.AddScoped<PlacesService>();
builder.Services.AddScoped<PlacesRepository>();
builder.Services.AddScoped<PackagesService>();
builder.Services.AddScoped<PackagesRepository>();
builder.Services.AddScoped<HotelsService>();
builder.Services.AddScoped<HotelsRepository>();
builder.Services.AddScoped<ContactService>();
builder.Services.AddScoped<ContactRepository>();
builder.Services.AddScoped<StatsService>();
builder.Services.AddScoped<StatsRepository>();
builder.Services.AddScoped<InfrastructureService>();
builder.Services.AddScoped<InfrastructureRepository>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddScoped<IPasswordResetDelivery, PasswordResetDelivery>();
builder.Services.AddScoped<PasswordResetService>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<CatalogCache>();
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
    options.AddPolicy("PasswordResetPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0
            }));

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
if (builder.Configuration.GetValue("Workers:Enabled", true))
{
    builder.Services.AddHostedService<CleanupWorker>();
    builder.Services.AddHostedService<MetricsSnapshotWorker>();
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IConfiguration>((options, configuration) =>
    {
        var secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Missing Jwt:Secret configuration.");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = configuration["Jwt:Issuer"] ?? "TravelAgency.Api",
            ValidAudience = configuration["Jwt:Audience"] ?? "TravelAgency.Client",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseResponseCompression();
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
