using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Travel_Agency_Portal.Services;

namespace Travel_Agency_Portal.Tests;

public class JwtSigningKeyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("short")]
    [InlineData("TravelAgencySuperSecretKey2026!ChangeMe")]
    [InlineData("replace-with-long-random-secret-material1234")]
    public void PublishedApplication_RejectsMissingOrExampleKeys(string? secret)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            { ["Jwt:Secret"] = secret }).Build();
        Assert.Throws<InvalidOperationException>(() => new JwtSigningKey(config, new EnvironmentStub("Production"), NullLogger<JwtSigningKey>.Instance));
    }

    [Fact]
    public void Development_GeneratesDifferentKeysForDifferentProcesses()
    {
        var config = new ConfigurationBuilder().Build();
        var first = new JwtSigningKey(config, new EnvironmentStub("Development"), NullLogger<JwtSigningKey>.Instance);
        var second = new JwtSigningKey(config, new EnvironmentStub("Development"), NullLogger<JwtSigningKey>.Instance);
        Assert.Equal(512, first.Key.KeySize);
        Assert.NotEqual(first.Key.Key, second.Key.Key);
    }

    private sealed class EnvironmentStub(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
