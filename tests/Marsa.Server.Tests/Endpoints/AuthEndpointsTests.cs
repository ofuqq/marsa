using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Marsa.Application.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Marsa.Server.Tests.Endpoints;

public sealed class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _baseFactory;

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
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

    private WebApplicationFactory<Program> CreateFactoryWithAuthService(IAuthenticationService authService)
    {
        return _baseFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IAuthenticationService>();
                services.AddSingleton(authService);
            });
        });
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200_AndSetsSecureHttpOnlySessionCookie()
    {
        var fakeAuth = new FakeAuthService();
        var client = CreateFactoryWithAuthService(fakeAuth).CreateClient();

        var payload = new
        {
            email = "admin@example.com",
            password = "VerySecurePassword123!",
        };

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Check user payload
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(body);
        Assert.True(json.TryGetProperty("user", out var userEl));
        Assert.Equal("admin@example.com", userEl.GetProperty("email").GetString());

        // Zero secret leakage in payload
        Assert.DoesNotContain("VerySecurePassword123!", body);
        Assert.DoesNotContain("hash", body, StringComparison.OrdinalIgnoreCase);

        // Verify Set-Cookie header attributes
        Assert.True(response.Headers.Contains("Set-Cookie"));
        var setCookie = response.Headers.GetValues("Set-Cookie").First();

        Assert.Contains("marsa_session=", setCookie);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", setCookie, StringComparison.OrdinalIgnoreCase);

        // Prove no sensitive user info inside the cookie itself
        Assert.DoesNotContain("admin@example.com", setCookie);
        Assert.DoesNotContain("VerySecurePassword123!", setCookie);
    }

    [Fact]
    public async Task Login_SessionFixationProtection_IssuesNewSessionTokenOnEachLogin()
    {
        var fakeAuth = new FakeAuthService();
        var client = CreateFactoryWithAuthService(fakeAuth).CreateClient();

        var payload = new
        {
            email = "admin@example.com",
            password = "VerySecurePassword123!",
        };

        var res1 = await client.PostAsJsonAsync("/api/v1/auth/login", payload);
        var cookie1 = res1.Headers.GetValues("Set-Cookie").First();

        var res2 = await client.PostAsJsonAsync("/api/v1/auth/login", payload);
        var cookie2 = res2.Headers.GetValues("Set-Cookie").First();

        Assert.NotEqual(cookie1, cookie2);
    }

    [Fact]
    public async Task Logout_InvalidatesSessionServerSide_AndClearsCookie()
    {
        var fakeAuth = new FakeAuthService();
        var client = CreateFactoryWithAuthService(fakeAuth).CreateClient();

        // 1. Login to obtain session cookie
        var loginRes = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@example.com",
            password = "VerySecurePassword123!",
        });
        Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);
        var sessionCookie = loginRes.Headers.GetValues("Set-Cookie").First().Split(';')[0];

        // 2. Fetch antiforgery token (cookie container automatically stores antiforgery cookie)
        var afRes = await client.GetAsync("/api/v1/auth/antiforgery");
        var afJson = await afRes.Content.ReadFromJsonAsync<JsonElement>();
        var afToken = afJson.GetProperty("token").GetString();

        // 3. Logout with antiforgery header (client sends cookies automatically)
        var logoutReq = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        logoutReq.Headers.Add("X-CSRF-TOKEN", afToken);

        var logoutRes = await client.SendAsync(logoutReq);
        Assert.Equal(HttpStatusCode.NoContent, logoutRes.StatusCode);

        // Verify cookie clearing in response
        var logoutCookies = logoutRes.Headers.GetValues("Set-Cookie");
        Assert.Contains(logoutCookies, c => c.Contains("marsa_session=;") || c.Contains("expires="));

        // 4. Verify server-side invalidation: fresh client using the old cookie gets 401
        var freshClient = CreateFactoryWithAuthService(fakeAuth).CreateClient();
        var meReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        meReq.Headers.Add("Cookie", sessionCookie);
        var meRes = await freshClient.SendAsync(meReq);

        Assert.Equal(HttpStatusCode.Unauthorized, meRes.StatusCode);
    }

    [Fact]
    public async Task Antiforgery_OnStateChangingCookieRequest_FailsWithoutToken()
    {
        var fakeAuth = new FakeAuthService();
        var client = CreateFactoryWithAuthService(fakeAuth).CreateClient();

        // Login to populate cookie container
        var loginRes = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@example.com",
            password = "VerySecurePassword123!",
        });
        Assert.Equal(HttpStatusCode.OK, loginRes.StatusCode);

        // Call POST /logout without X-CSRF-TOKEN header
        var logoutRes = await client.PostAsync("/api/v1/auth/logout", null);

        Assert.Equal(HttpStatusCode.BadRequest, logoutRes.StatusCode);
        var body = await logoutRes.Content.ReadAsStringAsync();
        Assert.Contains("Antiforgery", body);
    }

    [Fact]
    public async Task Antiforgery_BearerRequests_AreExemptFromAntiforgery()
    {
        var fakeAuth = new FakeAuthService();
        var client = CreateFactoryWithAuthService(fakeAuth).CreateClient();

        // Call POST with Bearer header (no cookie, no CSRF token)
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "mock-token-xyz");

        var res = await client.SendAsync(req);

        // Does not fail with 400 Antiforgery
        Assert.NotEqual(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Login_WhenUserDoesNotExist_Returns401_WithGenericMessage()
    {
        var fakeAuth = new FakeAuthService();
        var client = CreateFactoryWithAuthService(fakeAuth).CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "nonexistent@example.com",
            password = "WrongPassword123!",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid credentials", body);
        Assert.DoesNotContain("nonexistent@example.com", body);
    }

    [Fact]
    public async Task Login_WhenExceedingFailedAttempts_LocksAccountTemporarily()
    {
        var fakeAuth = new FakeAuthService { MaxAttemptsBeforeLockout = 3 };
        var client = CreateFactoryWithAuthService(fakeAuth).CreateClient();

        var payload = new
        {
            email = "admin@example.com",
            password = "IncorrectPassword!",
        };

        // 3 failed attempts
        for (var i = 0; i < 3; i++)
        {
            var res = await client.PostAsJsonAsync("/api/v1/auth/login", payload);
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        // 4th attempt triggers temporary lockout (429) before rate limit window is exhausted
        var lockRes = await client.PostAsJsonAsync("/api/v1/auth/login", payload);
        Assert.Equal(HttpStatusCode.TooManyRequests, lockRes.StatusCode);
        var body = await lockRes.Content.ReadAsStringAsync();
        Assert.Contains("locked", body);
    }

    [Fact]
    public async Task SecurityHeaders_ArePresentOnResponses()
    {
        var client = _baseFactory.CreateClient();

        var response = await client.GetAsync("/healthz");

        Assert.True(response.Headers.Contains("Content-Security-Policy"));
        var csp = response.Headers.GetValues("Content-Security-Policy").First();
        Assert.Contains("default-src 'self'", csp);
        Assert.Contains("frame-ancestors 'none'", csp);

        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());

        Assert.True(response.Headers.Contains("Referrer-Policy"));
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").First());

        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").First());
    }

    [Fact]
    public async Task SecurityHeaders_AuthenticatedResponse_SetsNoStoreCacheControl()
    {
        var fakeAuth = new FakeAuthService();
        var client = CreateFactoryWithAuthService(fakeAuth).CreateClient();

        // Login
        var loginRes = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@example.com",
            password = "VerySecurePassword123!",
        });
        var cookieHeader = loginRes.Headers.GetValues("Set-Cookie").First().Split(';')[0];

        // Call /me with session cookie
        var meReq = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        meReq.Headers.Add("Cookie", cookieHeader);
        var meRes = await client.SendAsync(meReq);

        Assert.Equal(HttpStatusCode.OK, meRes.StatusCode);
        Assert.True(meRes.Headers.CacheControl?.NoStore);
    }

    private sealed class FakeAuthService : IAuthenticationService
    {
        private static readonly Guid StableUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid StableOrgId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        public int FailedAttemptsCount { get; private set; }
        public int MaxAttemptsBeforeLockout { get; set; } = 5;
        private readonly HashSet<string> _revokedSessions = new();
        private int _sessionSequence;

        public Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
        {
            if (FailedAttemptsCount >= MaxAttemptsBeforeLockout)
            {
                return Task.FromResult(new LoginResult(false, true, null, null, "Account is temporarily locked."));
            }

            if (command.Email == "admin@example.com" && command.Password == "VerySecurePassword123!")
            {
                _sessionSequence++;
                var token = $"marsa_sess_tokensequence_{_sessionSequence}";
                var user = new AuthenticatedUser(
                    StableUserId,
                    command.Email,
                    "Administrator",
                    StableOrgId,
                    "owner");

                return Task.FromResult(new LoginResult(true, false, token, user, null));
            }

            FailedAttemptsCount++;
            return Task.FromResult(new LoginResult(false, false, null, null, "Invalid credentials."));
        }

        public Task<SessionValidationResult> ValidateSessionAsync(string sessionToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sessionToken) || _revokedSessions.Contains(sessionToken))
            {
                return Task.FromResult(new SessionValidationResult(false, null));
            }

            var user = new AuthenticatedUser(
                StableUserId,
                "admin@example.com",
                "Administrator",
                StableOrgId,
                "owner");

            return Task.FromResult(new SessionValidationResult(true, user));
        }

        public Task RevokeSessionAsync(string sessionToken, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(sessionToken))
            {
                _revokedSessions.Add(sessionToken);
            }

            return Task.CompletedTask;
        }
    }
}
