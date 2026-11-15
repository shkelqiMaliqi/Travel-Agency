using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Travel_Agency_Portal.Models;
using Travel_Agency_Portal.Services;

namespace Travel_Agency_Portal.Tests;

// Every test run gets a new database. The developer's Travel_Agency database is never modified.
public sealed class SqlApiFixture : IAsyncLifetime
{
    private readonly string _name = "TravelAgencyTests_" + Guid.NewGuid().ToString("N");
    private string _master = "";
    public string ConnectionString { get; private set; } = "";
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public CaptureResetDelivery Delivery { get; } = new();

    public async Task InitializeAsync()
    {
        var root = FindRoot();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(root, "Travel Agency Portal", "Travel Agency Portal", "appsettings.json"))
            .Build();
        var builder = new SqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION") ?? configuration.GetConnectionString("CRUDCS"))
        { InitialCatalog = "master", ConnectTimeout = 15, TrustServerCertificate = true };
        _master = builder.ConnectionString;
        builder.InitialCatalog = _name;
        ConnectionString = builder.ConnectionString;
        using var connection = new SqlConnection(_master);
        await connection.OpenAsync();
        using (var command = new SqlCommand($"CREATE DATABASE [{_name}]", connection))
            await command.ExecuteNonQueryAsync();
        var script = await File.ReadAllTextAsync(Path.Combine(root, "Database", "Travel_AgencyDB.sql"));
        script = Regex.Replace(script, @"^CREATE DATABASE Travel_Agency;\s*GO", "", RegexOptions.IgnoreCase);
        script = script.Replace("USE Travel_Agency;", $"USE [{_name}];");
        foreach (var batch in Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(batch)) continue;
            using var command = new SqlCommand(batch, connection) { CommandTimeout = 60 };
            await command.ExecuteNonQueryAsync();
        }
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(web =>
        {
            web.UseEnvironment("Testing");
            web.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CRUDCS"] = ConnectionString,
                ["Redis:ConnectionString"] = "",
                ["RabbitMq:ConnectionString"] = "",
                ["Mongo:ConnectionString"] = "",
                ["Logging:Logstash:Host"] = "",
                ["Workers:Enabled"] = "false",
                ["Security:MfaRequiredRoles"] = "",
                ["Jwt:Secret"] = "IsolatedIntegrationTestsOnlySigningKey1234567890!"
            }));
            web.ConfigureTestServices(services =>
            {
                services.RemoveAll<IPasswordResetDelivery>();
                services.AddSingleton<IPasswordResetDelivery>(Delivery);
            });
        });
    }

    public HttpClient Client(string? role = null, int userId = 1)
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        if (role is not null)
        {
            using var scope = Factory.Services.CreateScope();
            var token = scope.ServiceProvider.GetRequiredService<JwtTokenService>().CreateToken(new Users
            { U_Id = userId, U_Type = role, U_Email = "test@example.com", U_Name = "Test", U_Surname = "Account" });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        }
        return client;
    }

    public async Task<int> NewPackageAsync(int seats)
    {
        var suffix = Guid.NewGuid().ToString("N");
        return await ScalarAsync<int>("""
            INSERT INTO dbo.Places (Place_Name, Place_Description) VALUES (@Name, 'Integration fixture');
            DECLARE @Place int = SCOPE_IDENTITY();
            INSERT INTO dbo.Hotels (Place_Id, Hotel_Name, Hotel_Description, Hotel_Stars)
                VALUES (@Place, @Name, 'Integration fixture', 4);
            DECLARE @Hotel int = SCOPE_IDENTITY();
            INSERT INTO dbo.Travel_Packages
                (Place_Id, Hotel_Id, Package_Name, Package_Description, Price_Per_Person, Start_Date, End_Date, Available_Seats)
                OUTPUT INSERTED.Package_Id
                VALUES (@Place, @Hotel, @Name, 'Integration fixture', 125, '2030-01-01', '2030-01-10', @Seats);
            """, new SqlParameter("@Name", suffix), new SqlParameter("@Seats", seats));
    }

    public async Task<T> ScalarAsync<T>(string sql, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        return (T)Convert.ChangeType((await command.ExecuteScalarAsync())!, typeof(T));
    }

    public async Task DisposeAsync()
    {
        if (Factory is not null) await Factory.DisposeAsync();
        SqlConnection.ClearAllPools();
        if (string.IsNullOrEmpty(_master)) return;
        using var connection = new SqlConnection(_master);
        await connection.OpenAsync();
        using var command = new SqlCommand($"ALTER DATABASE [{_name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_name}];", connection);
        await command.ExecuteNonQueryAsync();
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Database", "Travel_AgencyDB.sql")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate the repository's database schema.");
    }
}

public sealed class CaptureResetDelivery : IPasswordResetDelivery
{
    public Dictionary<string, string> Codes { get; } = new();
    public Task SendAsync(string email, string token, DateTime expiresAt)
    {
        Codes[email] = token;
        return Task.CompletedTask;
    }
}
