namespace Marsa.Application.Setup;

/// <summary>
/// Service managing first-run system setup and administrator initialization.
/// </summary>
public interface ISetupService
{
    Task<bool> IsSetupCompletedAsync(CancellationToken cancellationToken = default);
    Task<SetupTokenGenerationResult?> GetOrCreateSetupTokenAsync(CancellationToken cancellationToken = default);
    Task<SetupResult> CompleteSetupAsync(SetupCommand command, CancellationToken cancellationToken = default);
}
