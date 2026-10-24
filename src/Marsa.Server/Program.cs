using Marsa.Server.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<MarsaOptions>()
    .Bind(builder.Configuration.GetSection("Marsa"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

// Resolving the validated options during startup makes invalid configuration
// fail before the HTTP pipeline starts. Option values are never logged.
_ = app.Services.GetRequiredService<IOptions<MarsaOptions>>().Value;

app.Run();
