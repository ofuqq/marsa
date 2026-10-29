using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Marsa.Application.Authentication;
using Marsa.Infrastructure.Persistence;
using Marsa.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Marsa.Infrastructure.Authentication;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly MarsaDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthenticationService> _logger;

    private static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(60);
    private static readonly TimeSpan AbsoluteTimeout = TimeSpan.FromHours(24);
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);
    private const int MaxFailedAttempts = 5;

    private static readonly ConcurrentDictionary<string, LockoutInfo> Lockouts = new();

    public AuthenticationService(
        MarsaDbContext dbContext,
        IPasswordHasher passwordHasher,
        ILogger<AuthenticationService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var email = command.Email?.Trim() ?? string.Empty;
        var normalizedEmail = email.ToUpperInvariant();
        var now = DateTimeOffset.UtcNow;

        // Check lockout
        if (IsLockedOut(normalizedEmail, now))
        {
            _logger.LogWarning("Login attempt for {Email} rejected due to active temporary lockout.", email);
            return new LoginResult(false, true, null, null, "Account is temporarily locked. Please try again later.");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user == null)
        {
            // Execute dummy verification to guarantee identical execution time (prevents user enumeration)
            _passwordHasher.VerifyDummyPassword(command.Password);
            RecordFailedAttempt(normalizedEmail, now);
            return new LoginResult(false, false, null, null, "Invalid credentials.");
        }

        var (isValid, needsRehash) = _passwordHasher.VerifyPasswordWithRehash(user.PasswordHash, command.Password);
        if (!isValid)
        {
            var isLocked = RecordFailedAttempt(normalizedEmail, now);
            if (isLocked)
            {
                _logger.LogWarning("Account {Email} temporarily locked after repeated failed login attempts.", email);
                return new LoginResult(false, true, null, null, "Account is temporarily locked. Please try again later.");
            }

            return new LoginResult(false, false, null, null, "Invalid credentials.");
        }

        // Reset lockout tracking on success
        ClearLockout(normalizedEmail);

        // Password hash upgrade
        if (needsRehash)
        {
            user.PasswordHash = _passwordHasher.HashPassword(command.Password);
            user.UpdatedAt = now;
        }

        // Prevent session fixation: generate a fresh 256-bit cryptographically secure session token
        var randomBytes = new byte[32];
        RandomNumberGenerator.Fill(randomBytes);
        var base64 = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
        var sessionToken = $"marsa_sess_{base64}";
        var tokenHash = HashToken(sessionToken);

        var sessionRecord = new UserSessionRecord
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            CreatedAt = now,
            ExpiresAt = now.Add(AbsoluteTimeout),
            LastActivityAt = now,
            IsRevoked = false,
            RevokedAt = null,
        };

        _dbContext.UserSessions.Add(sessionRecord);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var membership = await _dbContext.Memberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == user.Id, cancellationToken);

        var authUser = new AuthenticatedUser(
            user.Id,
            user.Email,
            user.DisplayName,
            membership?.OrganizationId,
            membership?.Role);

        _logger.LogInformation("User {UserId} successfully logged in; session {SessionId} created.", user.Id, sessionRecord.Id);

        return new LoginResult(true, false, sessionToken, authUser, null);
    }

    public async Task<SessionValidationResult> ValidateSessionAsync(string sessionToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return new SessionValidationResult(false, null);
        }

        var tokenHash = HashToken(sessionToken);
        var now = DateTimeOffset.UtcNow;

        var session = await _dbContext.UserSessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.TokenHash == tokenHash, cancellationToken);

        if (session == null || session.IsRevoked)
        {
            return new SessionValidationResult(false, null);
        }

        // Enforce absolute timeout
        if (session.ExpiresAt <= now)
        {
            return new SessionValidationResult(false, null);
        }

        // Enforce idle timeout
        if (now - session.LastActivityAt > IdleTimeout)
        {
            return new SessionValidationResult(false, null);
        }

        // Update last activity
        session.LastActivityAt = now;
        await _dbContext.SaveChangesAsync(cancellationToken);

        var membership = await _dbContext.Memberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == session.UserId, cancellationToken);

        var authUser = new AuthenticatedUser(
            session.User!.Id,
            session.User.Email,
            session.User.DisplayName,
            membership?.OrganizationId,
            membership?.Role);

        return new SessionValidationResult(true, authUser);
    }

    public async Task RevokeSessionAsync(string sessionToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            return;
        }

        var tokenHash = HashToken(sessionToken);
        var session = await _dbContext.UserSessions
            .FirstOrDefaultAsync(s => s.TokenHash == tokenHash, cancellationToken);

        if (session != null && !session.IsRevoked)
        {
            session.IsRevoked = true;
            session.RevokedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Session {SessionId} server-side revoked on logout.", session.Id);
        }
    }

    private static byte[] HashToken(string token)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(token));
    }

    private static bool IsLockedOut(string key, DateTimeOffset now)
    {
        if (Lockouts.TryGetValue(key, out var info))
        {
            return now < info.LockedUntil;
        }

        return false;
    }

    private static bool RecordFailedAttempt(string key, DateTimeOffset now)
    {
        var info = Lockouts.AddOrUpdate(
            key,
            _ => new LockoutInfo(1, DateTimeOffset.MinValue, now),
            (_, existing) =>
            {
                if (now - existing.FirstAttemptAt > TimeSpan.FromMinutes(10))
                {
                    return new LockoutInfo(1, DateTimeOffset.MinValue, now);
                }

                var newCount = existing.FailedAttempts + 1;
                var lockedUntil = newCount >= MaxFailedAttempts ? now.Add(LockoutDuration) : DateTimeOffset.MinValue;
                return new LockoutInfo(newCount, lockedUntil, existing.FirstAttemptAt);
            });

        return now < info.LockedUntil;
    }

    private static void ClearLockout(string key)
    {
        Lockouts.TryRemove(key, out _);
    }

    private sealed record LockoutInfo(
        int FailedAttempts,
        DateTimeOffset LockedUntil,
        DateTimeOffset FirstAttemptAt
    );
}
