using Marsa.Application.Setup;

namespace Marsa.Server.Setup;

/// <summary>
/// Hosted service that ensures a one-time setup token is generated and logged to the operator
/// on first run when the system has not yet been set up.
/// </summary>
public sealed class SetupTokenBootstrapService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SetupTokenBootstrapService> _logger;

    public SetupTokenBootstrapService(
        IServiceProvider serviceProvider,
        ILogger<SetupTokenBootstrapService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var setupService = scope.ServiceProvider.GetRequiredService<ISetupService>();

            if (!await setupService.IsSetupCompletedAsync(cancellationToken))
            {
                var forceRegenerate = string.Equals(
                    Environment.GetEnvironmentVariable("MARSA_REGENERATE_SETUP_TOKEN"),
                    "true",
                    StringComparison.OrdinalIgnoreCase);

                var result = await setupService.GetOrCreateSetupTokenAsync(forceRegenerate, cancellationToken);
                if (result != null)
                {
                    var filePath = result.FilePath ?? "data directory (setup-token)";
                    _logger.LogInformation(
                        "[MARSA SETUP] Setup is required. One-time setup token written to restricted file '{FilePath}' (mode 0600). Token is NOT printed to logs. Expires at {ExpiresAt:u}.",
                        filePath,
                        result.ExpiresAt);

                    if (!string.Equals(Environment.GetEnvironmentVariable("MARSA_SUPPRESS_SETUP_CONSOLE_OUTPUT"), "true", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("================================================================================");
                        Console.WriteLine("[MARSA FIRST-RUN SETUP]");
                        Console.WriteLine("Setup is required. One-time setup token has been written to the restricted file:");
                        Console.WriteLine(filePath);
                        Console.WriteLine($"Expires at: {result.ExpiresAt:u}. File permissions: 0600 (read/write by owner only).");
                        Console.WriteLine("For security, the token is NEVER printed to container logs or stdout.");
                        Console.WriteLine("================================================================================");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Setup token bootstrap could not initialize; setup can be initialized on demand.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
