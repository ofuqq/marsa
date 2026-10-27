using Marsa.Infrastructure;
using Marsa.Infrastructure.Docker;
using Marsa.Server.Configuration;
using Marsa.Server.Errors;
using Marsa.Server.Health;
using Marsa.Server.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<MarsaOptions>()
    .Bind(builder.Configuration.GetSection("Marsa"))
    .PostConfigure(options =>
    {
        if (string.IsNullOrWhiteSpace(options.DatabaseConnectionString))
        {
            options.DatabaseConnectionString = builder.Configuration["Marsa:Database:ConnectionString"]
                ?? builder.Configuration["MARSA_Database__ConnectionString"]
                ?? string.Empty;
        }
        if (string.IsNullOrWhiteSpace(options.MasterKeyPath))
        {
            options.MasterKeyPath = builder.Configuration["Marsa:Security:MasterKeyPath"]
                ?? builder.Configuration["MARSA_Security__MasterKeyPath"]
                ?? string.Empty;
        }
        if (string.IsNullOrWhiteSpace(options.CaddyAdminSocket))
        {
            options.CaddyAdminSocket = builder.Configuration["Marsa:Edge:CaddyAdminSocket"]
                ?? builder.Configuration["MARSA_Edge__AdminSocket"]
                ?? string.Empty;
        }
        if (string.IsNullOrWhiteSpace(options.PublicBaseUrl))
        {
            options.PublicBaseUrl = builder.Configuration["Marsa:PublicBaseUrl"]
                ?? builder.Configuration["MARSA_PublicBaseUrl"]
                ?? string.Empty;
        }
    })
    .ValidateDataAnnotations()
    .ValidateOnStart();

var connectionString = builder.Configuration.GetSection("Marsa")["DatabaseConnectionString"]
    ?? builder.Configuration.GetSection("Marsa:Database")["ConnectionString"]
    ?? builder.Configuration["MARSA_Database__ConnectionString"]
    ?? string.Empty;

builder.Services.AddMarsaInfrastructure(connectionString);
builder.Services.AddHealthChecks()
    .AddCheck<DockerHealthCheck>("docker", tags: new[] { "ready" });

if (builder.Environment.IsProduction())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddJsonConsole();
}

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    await ProblemDetailsWriter.WriteAsync(context, new Exception("Unhandled"));
}));

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

var detailedHealthOptions = new HealthCheckOptions
{
    ResponseWriter = WriteHealthReportAsync,
};

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteHealthReportAsync,
});
app.MapHealthChecks("/health/ready", detailedHealthOptions);
app.MapHealthChecks("/health/degraded", detailedHealthOptions);

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
