namespace Marsa.Application.Authentication;

public sealed record LoginCommand(
    string Email,
    string Password
);

public sealed record AuthenticatedUser(
    Guid Id,
    string Email,
    string DisplayName,
    Guid? OrganizationId,
    string? Role
);

public sealed record LoginResult(
    bool Succeeded,
    bool IsLockedOut,
    string? SessionToken,
    AuthenticatedUser? User,
    string? ErrorMessage
);

public sealed record SessionValidationResult(
    bool IsValid,
    AuthenticatedUser? User
);
