namespace Marsa.Application.Setup;

public class SetupException(string message) : Exception(message);

public sealed class SetupAlreadyCompletedException()
    : SetupException("First-run setup has already been completed.");

public sealed class InvalidSetupTokenException()
    : SetupException("The provided setup token is invalid, expired, or has already been used.");

public sealed class ConcurrencySetupConflictException()
    : SetupException("Concurrent setup conflict: an administrator already exists.");
