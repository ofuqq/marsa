using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Marsa.Application.Setup;
using Marsa.Domain.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Marsa.Server.Tests.Endpoints;

public sealed class SetupEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _baseFactory;

    public SetupEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _baseFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Marsa:Database:ConnectionString", "Host=localhost;Database=marsa");
            builder.UseSetting("Marsa:Security:MasterKeyPath", "/tmp/marsa-master.key");
            builder.UseSetting("Marsa:Edge:CaddyAdminSocket", "/tmp/marsa-caddy.sock");
            builder.UseSetting("Marsa:PublicBaseUrl", "http://localhost:5080");
            Environment.SetEnvironmentVariable("MARSA_SUPPRESS_SETUP_CONSOLE_OUTPUT", "true");
        });
    }

    private WebApplicationFactory<Program> CreateFactoryWithSetupService(ISetupService setupService)
    {
        return _baseFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ISetupService>();
                services.AddSingleton(setupService);
            });
        });
    }

    [Fact]
    public async Task GetSetupStatus_WhenSetupPending_Returns200WithSetupRequired()
    {
        var fake = new FakeSetupService { IsCompleted = false };
        var client = CreateFactoryWithSetupService(fake).CreateClient();

        var response = await client.GetAsync("/system/setup");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("required").GetBoolean());
        Assert.True(json.GetProperty("tokenRequired").GetBoolean());
    }

    [Fact]
    public async Task GetSetupStatus_WhenSetupAlreadyCompleted_Returns404NotFoundMaskingState()
    {
        var fake = new FakeSetupService { IsCompleted = true };
        var client = CreateFactoryWithSetupService(fake).CreateClient();

        var response = await client.GetAsync("/system/setup");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostSetup_WithValidPayloadAndToken_Returns201CreatedWithoutExposingSecrets()
    {
        var fake = new FakeSetupService { IsCompleted = false, ValidToken = "marsa_st_validtoken1234567890abcdef" };
        var client = CreateFactoryWithSetupService(fake).CreateClient();

        var payload = new
        {
            token = "marsa_st_validtoken1234567890abcdef",
            email = "admin@example.com",
            password = "VerySecurePassword123!",
            displayName = "Administrator",
            organizationName = "Acme Corp",
            organizationSlug = "acme-corp",
        };

        var response = await client.PostAsJsonAsync("/system/setup", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("/organizations/acme-corp", response.Headers.Location.OriginalString);

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.True(json.TryGetProperty("organizationId", out _));
        Assert.True(json.TryGetProperty("administratorId", out _));
        Assert.Equal("acme-corp", json.GetProperty("organizationSlug").GetString());

        // Zero secret leakage check
        Assert.DoesNotContain("marsa_st_", body);
        Assert.DoesNotContain("VerySecurePassword123!", body);
        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PostSetup_WithInvalidToken_Returns401Unauthorized()
    {
        var fake = new FakeSetupService { IsCompleted = false, ValidToken = "marsa_st_validtoken1234567890abcdef" };
        var client = CreateFactoryWithSetupService(fake).CreateClient();

        var payload = new
        {
            token = "marsa_st_wrongtoken000000000000000000",
            email = "admin@example.com",
            password = "VerySecurePassword123!",
            displayName = "Administrator",
            organizationName = "Acme Corp",
            organizationSlug = "acme-corp",
        };

        var response = await client.PostAsJsonAsync("/system/setup", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostSetup_WhenPasswordShorterThan12Chars_Returns400BadRequest()
    {
        var fake = new FakeSetupService { IsCompleted = false, ValidToken = "marsa_st_validtoken1234567890abcdef" };
        var client = CreateFactoryWithSetupService(fake).CreateClient();

        var payload = new
        {
            token = "marsa_st_validtoken1234567890abcdef",
            email = "admin@example.com",
            password = "short", // Only 5 characters, violates min 12
            displayName = "Administrator",
            organizationName = "Acme Corp",
            organizationSlug = "acme-corp",
        };

        var response = await client.PostAsJsonAsync("/system/setup", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("at least 12 characters", body);
    }

    [Fact]
    public async Task PostSetup_WhenSlugInvalid_Returns400BadRequest()
    {
        var fake = new FakeSetupService { IsCompleted = false, ValidToken = "marsa_st_validtoken1234567890abcdef" };
        var client = CreateFactoryWithSetupService(fake).CreateClient();

        var payload = new
        {
            token = "marsa_st_validtoken1234567890abcdef",
            email = "admin@example.com",
            password = "VerySecurePassword123!",
            displayName = "Administrator",
            organizationName = "Acme Corp",
            organizationSlug = "INVALID_SLUG_WITH_CAPS!", // Invalid DNS label
        };

        var response = await client.PostAsJsonAsync("/system/setup", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("DNS label", body);
    }

    [Fact]
    public async Task PostSetup_WhenAlreadyCompleted_Returns404NotFoundMaskingState()
    {
        var fake = new FakeSetupService { IsCompleted = true };
        var client = CreateFactoryWithSetupService(fake).CreateClient();

        var payload = new
        {
            token = "marsa_st_validtoken1234567890abcdef",
            email = "admin@example.com",
            password = "VerySecurePassword123!",
            displayName = "Administrator",
            organizationName = "Acme Corp",
            organizationSlug = "acme-corp",
        };

        var response = await client.PostAsJsonAsync("/system/setup", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostSetup_ConcurrentFirstSetupRequests_CreatesAtMostOneAdministrator()
    {
        var fake = new FakeSetupService { IsCompleted = false, ValidToken = "marsa_st_validtoken1234567890abcdef" };
        var client = CreateFactoryWithSetupService(fake).CreateClient();

        var payload = new
        {
            token = "marsa_st_validtoken1234567890abcdef",
            email = "admin@example.com",
            password = "VerySecurePassword123!",
            displayName = "Administrator",
            organizationName = "Acme Corp",
            organizationSlug = "acme-corp",
        };

        // Fire two concurrent requests
        var task1 = client.PostAsJsonAsync("/system/setup", payload);
        var task2 = client.PostAsJsonAsync("/system/setup", payload);

        var responses = await Task.WhenAll(task1, task2);

        var statusCodes = responses.Select(r => r.StatusCode).ToList();

        // Exactly one must succeed (201 Created), and the concurrent conflicting request must receive 404
        Assert.Contains(HttpStatusCode.Created, statusCodes);
        Assert.Contains(HttpStatusCode.NotFound, statusCodes);
        Assert.Equal(1, fake.SuccessfulCompletionsCount);
    }

    [Fact]
    public async Task SetupEndpoints_EnforcesRateLimiter()
    {
        var fake = new FakeSetupService { IsCompleted = false };
        var client = CreateFactoryWithSetupService(fake).CreateClient();

        // Rate limiter allows 5 requests per window (Window = 1 minute, PermitLimit = 5)
        var responses = new List<HttpResponseMessage>();
        for (var i = 0; i < 6; i++)
        {
            responses.Add(await client.GetAsync("/system/setup"));
        }

        // First 5 requests should pass (200 OK)
        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(HttpStatusCode.OK, responses[i].StatusCode);
        }

        // 6th request must be rate-limited (429 Too Many Requests)
        Assert.Equal(HttpStatusCode.TooManyRequests, responses[5].StatusCode);
    }

    private sealed class FakeSetupService : ISetupService
    {
        public bool IsCompleted { get; set; }
        public string ValidToken { get; set; } = "marsa_st_default_token";
        public int SuccessfulCompletionsCount { get; private set; }

        private readonly object _lock = new();

        public Task<bool> IsSetupCompletedAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(IsCompleted);
        }

        public Task<SetupTokenGenerationResult?> GetOrCreateSetupTokenAsync(CancellationToken cancellationToken = default)
        {
            if (IsCompleted)
            {
                return Task.FromResult<SetupTokenGenerationResult?>(null);
            }

            return Task.FromResult<SetupTokenGenerationResult?>(
                new SetupTokenGenerationResult(ValidToken, DateTimeOffset.UtcNow.AddHours(24)));
        }

        public Task<SetupResult> CompleteSetupAsync(SetupCommand command, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (IsCompleted)
                {
                    throw new SetupAlreadyCompletedException();
                }

                if (!string.Equals(command.Token, ValidToken, StringComparison.Ordinal))
                {
                    throw new InvalidSetupTokenException();
                }

                // Simulate atomic completion and singleton constraint
                IsCompleted = true;
                SuccessfulCompletionsCount++;

                return Task.FromResult(new SetupResult(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    command.OrganizationSlug));
            }
        }
    }
}
