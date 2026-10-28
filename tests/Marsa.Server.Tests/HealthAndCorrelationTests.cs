using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Marsa.Server.Tests;

public class HealthAndCorrelationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory.WithWebHostBuilder(builder =>
    {
        builder.UseSetting("Marsa:Database:ConnectionString", "Host=localhost;Database=marsa");
        builder.UseSetting("Marsa:Security:MasterKeyPath", "/tmp/marsa-master.key");
        builder.UseSetting("Marsa:Edge:CaddyAdminSocket", "/tmp/marsa-caddy.sock");
        builder.UseSetting("Marsa:PublicBaseUrl", "http://localhost:5080");
    });

    [Fact]
    public async Task LivePreservesValidCorrelationId()
    {
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("X-Request-Id", "abc-123");

        var response = await client.SendAsync(request);

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal("abc-123", response.Headers.GetValues("X-Request-Id").Single());
    }

    [Fact]
    public async Task LiveCreatesCorrelationIdWhenHeaderIsMissing()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.True(response.IsSuccessStatusCode);
        Assert.True(response.Headers.Contains("X-Request-Id"));
    }

    [Fact]
    public async Task DegradedReportsDockerComponent()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/degraded");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("docker", body, StringComparison.OrdinalIgnoreCase);
    }
}
