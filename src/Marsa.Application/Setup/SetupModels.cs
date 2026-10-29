namespace Marsa.Application.Setup;

public sealed record SetupCommand(
    string Token,
    string Email,
    string Password,
    string DisplayName,
    string OrganizationName,
    string OrganizationSlug
);

public sealed record SetupResult(
    Guid OrganizationId,
    Guid AdministratorId,
    string OrganizationSlug
);

public sealed record SetupTokenGenerationResult(
    string PlaintextToken,
    DateTimeOffset ExpiresAt,
    string? FilePath = null
);
