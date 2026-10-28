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
                var result = await setupService.GetOrCreateSetupTokenAsync(cancellationToken);
                if (result != null && !string.Equals(Environment.GetEnvironmentVariable("MARSA_SUPPRESS_SETUP_CONSOLE_OUTPUT"), "true", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("================================================================================");
                    Console.WriteLine("[MARSA FIRST-RUN SETUP]");
                    Console.WriteLine("Setup is required. One-time setup token:");
                    Console.WriteLine(result.PlaintextToken);
                    Console.WriteLine($"Expires at: {result.ExpiresAt:u}. This token can only be used once.");
                    Console.WriteLine("================================================================================");
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
