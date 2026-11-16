using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Travel_Agency_Portal.Tests;

public class ApiInfrastructureTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiInfrastructureTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(web => web.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "InfrastructureTestsOnlySigningKey1234567890!",
                ["Workers:Enabled"] = "false",
                ["ConnectionStrings:CRUDCS"] = "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true"
            })));
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_RequiresAuthentication()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/v1/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
