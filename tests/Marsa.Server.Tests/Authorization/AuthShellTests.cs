using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Marsa.Application.Authentication;
using Marsa.Application.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Marsa.Server.Tests.Authorization;

public sealed class AuthShellTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _baseFactory;

    public AuthShellTests(WebApplicationFactory<Program> factory)
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

    private WebApplicationFactory<Program> CreateFactoryWithUserRole(string role, string token = "test_token")
    {
        var fakeAuth = new TestRoleAuthService(role, token);
        return _baseFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IAuthenticationService>();
                services.AddSingleton<IAuthenticationService>(fakeAuth);
            });
        });
    }

    [Fact]
    public async Task DenyByDefault_EndpointWithoutExplicitPolicy_Returns401_ProvingEndpointIsNotPublic()
    {
        var client = _baseFactory.CreateClient();

        // /api/v1/system/info is mapped without any explicit authorization attribute or AllowAnonymous
        var response = await client.GetAsync("/api/v1/system/info");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DenyByDefault_UnauthenticatedApiRequest_Returns401ProblemDetails()
    {
        var client = _baseFactory.CreateClient();

        // Call protected API endpoint without session
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(body);
        Assert.Equal(401, json.GetProperty("status").GetInt32());
        Assert.Equal("Unauthorized", json.GetProperty("title").GetString());
    }

    [Fact]
    public async Task DenyByDefault_UnauthenticatedBrowserNavigation_RedirectsToLogin()
    {
        var client = _baseFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("/login", response.Headers.Location.OriginalString);
    }

    [Theory]
    [InlineData("/healthz")]
    [InlineData("/health/live")]
    [InlineData("/login")]
    public async Task ExplicitAnonymous_PermittedEndpoints_AreAccessibleWithoutAuthentication(string endpoint)
    {
        var client = _baseFactory.CreateClient();

        var response = await client.GetAsync(endpoint);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RolePolicy_Viewer_IsForbiddenFromAdminView()
    {
        var factory = CreateFactoryWithUserRole(MarsaRoles.Viewer, "viewer_token");
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var request = new HttpRequestMessage(HttpMethod.Get, "/admin");
        request.Headers.Add("Cookie", "marsa_session=viewer_token");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RolePolicy_Developer_IsForbiddenFromAdminView()
    {
        var factory = CreateFactoryWithUserRole(MarsaRoles.Developer, "dev_token");
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var request = new HttpRequestMessage(HttpMethod.Get, "/admin");
        request.Headers.Add("Cookie", "marsa_session=dev_token");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(MarsaRoles.Admin)]
    [InlineData(MarsaRoles.Owner)]
    public async Task RolePolicy_AdminAndOwner_CanAccessAdminView(string role)
    {
        var token = $"{role}_token";
        var factory = CreateFactoryWithUserRole(role, token);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var request = new HttpRequestMessage(HttpMethod.Get, "/admin");
        request.Headers.Add("Cookie", $"marsa_session={token}");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Administrator Area", html);
    }

    [Fact]
    public async Task UiShell_WhenAuthenticated_RendersServerSideWithAccessibleLandmarks()
    {
        var factory = CreateFactoryWithUserRole(MarsaRoles.Owner, "owner_token");
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("Cookie", "marsa_session=owner_token");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();

        // Server-rendered HTML structure
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<main id=\"main-content\"", html);
        Assert.Contains("role=\"main\"", html);
        Assert.Contains("<nav aria-label=\"Main Navigation\"", html);
        Assert.Contains("class=\"skip-link\"", html);

        // Authenticated user and role rendered server-side
        Assert.Contains("Owner User", html);
        Assert.Contains("owner", html);

        // Verify zero external Node/SPA script tags
        Assert.DoesNotContain("<script src=\"/node_modules", html);
        Assert.DoesNotContain("<script src=\"/_framework/blazor.webassembly", html);
    }

    private sealed class TestRoleAuthService : IAuthenticationService
    {
        private readonly string _role;
        private readonly string _token;

        public TestRoleAuthService(string role, string token)
        {
            _role = role;
            _token = token;
        }

        public Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new LoginResult(false, false, null, null, "Not used in test"));
        }

        public Task<SessionValidationResult> ValidateSessionAsync(string sessionToken, CancellationToken cancellationToken = default)
        {
            if (sessionToken == _token)
            {
                var user = new AuthenticatedUser(
                    Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    $"{_role}@example.com",
                    $"{char.ToUpperInvariant(_role[0])}{_role[1..]} User",
                    Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    _role);

                return Task.FromResult(new SessionValidationResult(true, user));
            }

            return Task.FromResult(new SessionValidationResult(false, null));
        }

        public Task RevokeSessionAsync(string sessionToken, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

