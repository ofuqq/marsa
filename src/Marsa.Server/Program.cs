using System.Net;
using System.Threading.RateLimiting;
using Marsa.Application.Authorization;
using Marsa.Infrastructure;
using Marsa.Infrastructure.Docker;
using Marsa.Server.Authentication;
using Marsa.Server.Components;
using Marsa.Server.Configuration;
using Marsa.Server.Endpoints;
using Marsa.Server.Errors;
using Marsa.Server.Health;
using Marsa.Server.Middleware;
using Marsa.Server.Setup;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<MarsaOptions>()
    .Bind(builder.Configuration.GetSection("Marsa"))
    .PostConfigure(options =>
    {
        if (string.IsNullOrWhiteSpace(options.DatabaseConnectionString))
        {
            options.DatabaseConnectionString = builder.Configuration["Marsa:Database:ConnectionString"] ?? string.Empty;
        }
        if (string.IsNullOrWhiteSpace(options.MasterKeyPath))
        {
            options.MasterKeyPath = builder.Configuration["Marsa:Security:MasterKeyPath"] ?? string.Empty;
        }
        if (string.IsNullOrWhiteSpace(options.CaddyAdminSocket))
        {
            options.CaddyAdminSocket = builder.Configuration["Marsa:Edge:CaddyAdminSocket"] ?? string.Empty;
        }
        if (string.IsNullOrWhiteSpace(options.PublicBaseUrl))
        {
            options.PublicBaseUrl = builder.Configuration["Marsa:PublicBaseUrl"] ?? string.Empty;
        }
    })
    .ValidateDataAnnotations()
    .ValidateOnStart();

var connectionString = builder.Configuration["Marsa:Database:ConnectionString"] ?? string.Empty;

builder.Services.AddMarsaInfrastructure(connectionString);
builder.Services.AddHealthChecks()
    .AddCheck<DockerHealthCheck>("docker", tags: new[] { "ready" });

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Clear();
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Add(IPAddress.Loopback);
    options.KnownProxies.Add(IPAddress.IPv6Loopback);

    var trustedProxiesConfig = builder.Configuration.GetSection("Marsa:Edge:TrustedProxies").Get<string[]>()
        ?? builder.Configuration.GetSection("Marsa:TrustedProxies").Get<string[]>()
        ?? Array.Empty<string>();

    foreach (var proxy in trustedProxiesConfig)
    {
        if (IPAddress.TryParse(proxy, out var ip))
        {
            options.KnownProxies.Add(ip);
        }
    }
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = MarsaAuthenticationHandler.SchemeName;
    options.DefaultChallengeScheme = MarsaAuthenticationHandler.SchemeName;
    options.DefaultForbidScheme = MarsaAuthenticationHandler.SchemeName;
})
.AddScheme<AuthenticationSchemeOptions, MarsaAuthenticationHandler>(MarsaAuthenticationHandler.SchemeName, _ => { });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy(MarsaPolicies.RequireOwner, policy =>
        policy.RequireRole(MarsaRoles.Owner));

    options.AddPolicy(MarsaPolicies.RequireAdmin, policy =>
        policy.RequireRole(MarsaRoles.Owner, MarsaRoles.Admin));

    options.AddPolicy(MarsaPolicies.RequireDeveloper, policy =>
        policy.RequireRole(MarsaRoles.Owner, MarsaRoles.Admin, MarsaRoles.Developer));

    options.AddPolicy(MarsaPolicies.RequireViewer, policy =>
        policy.RequireRole(MarsaRoles.Owner, MarsaRoles.Admin, MarsaRoles.Developer, MarsaRoles.Viewer));
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddRazorComponents();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("setup", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            clientIp,
            _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 5,
                QueueLimit = 0,
            });
    });
    options.AddPolicy("login", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            clientIp,
            _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 5,
                QueueLimit = 0,
            });
    });
});

builder.Services.AddHostedService<SetupTokenBootstrapService>();

if (builder.Environment.IsProduction())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddJsonConsole();
}

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseForwardedHeaders();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    await ProblemDetailsWriter.WriteAsync(context, new Exception("Unhandled"));
}));

app.UseRateLimiter();
app.UseMiddleware<SessionAuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseMiddleware<CookieAntiforgeryValidationMiddleware>();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

var detailedHealthOptions = new HealthCheckOptions
{
    ResponseWriter = WriteHealthReportAsync,
};

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteHealthReportAsync,
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", detailedHealthOptions).AllowAnonymous();
app.MapHealthChecks("/health/degraded", detailedHealthOptions).AllowAnonymous();

app.MapSetupEndpoints();
app.MapAuthEndpoints();

app.MapGet("/api/v1/system/info", () => Results.Ok(new { version = "0.1.0" }));

app.MapRazorComponents<App>();

_ = app.Services.GetRequiredService<IOptions<MarsaOptions>>().Value;

app.Run();

static async Task WriteHealthReportAsync(HttpContext context, HealthReport report)
{
    var payload = new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            description = entry.Value.Description,
        }),
    };

    context.Response.ContentType = "application/json";
    await System.Text.Json.JsonSerializer.SerializeAsync(context.Response.Body, payload);
}
