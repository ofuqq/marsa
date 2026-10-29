using Marsa.Application.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Marsa.Infrastructure.Authentication;

public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();
    private static readonly object DummyUser = new();
    private static readonly string PrecomputedDummyHash = new PasswordHasher<object>().HashPassword(DummyUser, "DummyTimingProtectionPassword123!");

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return _hasher.HashPassword(DummyUser, password);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(providedPassword))
        {
            return false;
        }

        var result = _hasher.VerifyHashedPassword(DummyUser, hashedPassword, providedPassword);
        return result != PasswordVerificationResult.Failed;
    }

    public (bool IsValid, bool NeedsRehash) VerifyPasswordWithRehash(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(providedPassword))
        {
            return (false, false);
        }

        var result = _hasher.VerifyHashedPassword(DummyUser, hashedPassword, providedPassword);
        return (result != PasswordVerificationResult.Failed, result == PasswordVerificationResult.SuccessRehashNeeded);
    }

    public bool VerifyDummyPassword(string providedPassword)
    {
        _hasher.VerifyHashedPassword(DummyUser, PrecomputedDummyHash, providedPassword ?? string.Empty);
        return false;
    }
}
