using Marsa.Server.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Marsa.Server.Tests;

public class ConfigurationShapeTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public void EnvironmentVariableBindsToCanonicalConfigurationKey()
    {
        Environment.SetEnvironmentVariable("Marsa__Database__ConnectionString", "Host=testhost;Database=testdb");
        Environment.SetEnvironmentVariable("Marsa__Security__MasterKeyPath", "/tmp/test.key");
        Environment.SetEnvironmentVariable("Marsa__Edge__CaddyAdminSocket", "/tmp/test.sock");
        Environment.SetEnvironmentVariable("Marsa__PublicBaseUrl", "http://localhost:5080");

        var builder = WebApplication.CreateBuilder();

        Assert.Equal("Host=testhost;Database=testdb", builder.Configuration["Marsa:Database:ConnectionString"]);
        Assert.Equal("/tmp/test.key", builder.Configuration["Marsa:Security:MasterKeyPath"]);
        Assert.Equal("/tmp/test.sock", builder.Configuration["Marsa:Edge:CaddyAdminSocket"]);
        Assert.Equal("http://localhost:5080", builder.Configuration["Marsa:PublicBaseUrl"]);
    }

    [Fact]
    public void ServerOptionsBindCorrectlyWithCanonicalKeys()
    {
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Marsa:Database:ConnectionString", "Host=localhost;Database=marsa_test");
            builder.UseSetting("Marsa:Security:MasterKeyPath", "/tmp/marsa-master.key");
            builder.UseSetting("Marsa:Edge:CaddyAdminSocket", "/tmp/marsa-caddy.sock");
            builder.UseSetting("Marsa:PublicBaseUrl", "http://localhost:5080");
        });

        var options = customFactory.Services.GetRequiredService<IOptions<MarsaOptions>>().Value;

        Assert.Equal("Host=localhost;Database=marsa_test", options.DatabaseConnectionString);
        Assert.Equal("/tmp/marsa-master.key", options.MasterKeyPath);
        Assert.Equal("/tmp/marsa-caddy.sock", options.CaddyAdminSocket);
        Assert.Equal("http://localhost:5080", options.PublicBaseUrl);
    }
}
