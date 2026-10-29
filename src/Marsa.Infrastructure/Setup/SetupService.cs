using System.Data;
using System.Security.Cryptography;
using Marsa.Application.Authentication;
using Marsa.Application.Setup;
using Marsa.Domain.Setup;
using Marsa.Infrastructure.Persistence;
using Marsa.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Marsa.Infrastructure.Setup;

public sealed class SetupService : ISetupService
{
    private readonly MarsaDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<SetupService> _logger;

    public SetupService(
        MarsaDbContext dbContext,
        IPasswordHasher passwordHasher,
        ILogger<SetupService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<bool> IsSetupCompletedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SystemSetup.AnyAsync(s => s.IsCompleted, cancellationToken)
            || await _dbContext.Organizations.AnyAsync(o => o.SetupCompletedAt != null, cancellationToken);
    }

    public async Task<SetupTokenGenerationResult?> GetOrCreateSetupTokenAsync(bool forceRegenerate = false, CancellationToken cancellationToken = default)
    {
        if (await IsSetupCompletedAsync(cancellationToken))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        if (!forceRegenerate)
        {
            var existingToken = await _dbContext.SetupTokens
                .Where(t => t.UsedAt == null && t.ExpiresAt > now)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingToken != null && TokenFileExists())
            {
                return null;
            }
        }

        // Invalidate previous unconsumed tokens upon regeneration or recovery
        var previousTokens = await _dbContext.SetupTokens
            .Where(t => t.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var oldToken in previousTokens)
        {
            oldToken.UsedAt = now;
        }

        var (tokenEntity, plaintext) = SetupToken.Generate(TimeSpan.FromHours(24));

        var record = new SetupTokenRecord
        {
            Id = tokenEntity.Id,
            TokenHash = tokenEntity.TokenHash,
            CreatedAt = tokenEntity.CreatedAt,
            ExpiresAt = tokenEntity.ExpiresAt,
            UsedAt = null,
        };

        _dbContext.SetupTokens.Add(record);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var tokenPath = TryWriteRestrictedTokenFile(plaintext);

        _logger.LogInformation(
            "Setup is required. One-time setup token generated (expires at {ExpiresAt:u}). Written to restricted file {TokenPath}.",
            tokenEntity.ExpiresAt,
            tokenPath ?? "data directory");

        return new SetupTokenGenerationResult(plaintext, tokenEntity.ExpiresAt, tokenPath);
    }

    public async Task<SetupResult> CompleteSetupAsync(SetupCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Validate password using domain value object (enforces min length 12)
        var passwordVo = new Password(command.Password);

        if (string.IsNullOrWhiteSpace(command.Token))
        {
            throw new InvalidSetupTokenException();
        }

        if (string.IsNullOrWhiteSpace(command.Email) || !command.Email.Contains('@'))
        {
            throw new ArgumentException("A valid email address is required.", nameof(command.Email));
        }

        if (string.IsNullOrWhiteSpace(command.DisplayName) || command.DisplayName.Length > 120)
        {
            throw new ArgumentException("Display name must be between 1 and 120 characters.", nameof(command.DisplayName));
        }

        if (string.IsNullOrWhiteSpace(command.OrganizationName) || command.OrganizationName.Length > 120)
        {
            throw new ArgumentException("Organization name must be between 1 and 120 characters.", nameof(command.OrganizationName));
        }

        if (string.IsNullOrWhiteSpace(command.OrganizationSlug) || command.OrganizationSlug.Length > 63)
        {
            throw new ArgumentException("Organization slug must be between 1 and 63 characters.", nameof(command.OrganizationSlug));
        }

        var now = DateTimeOffset.UtcNow;
        var incomingHash = SetupToken.HashToken(command.Token);

        // Perform atomic setup in transaction
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var tx = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            if (await IsSetupCompletedAsync(cancellationToken))
            {
                throw new SetupAlreadyCompletedException();
            }

            var activeTokens = await _dbContext.SetupTokens
                .Where(t => t.UsedAt == null && t.ExpiresAt > now)
                .ToListAsync(cancellationToken);

            SetupTokenRecord? matchingToken = null;
            foreach (var candidate in activeTokens)
            {
                if (CryptographicOperations.FixedTimeEquals(candidate.TokenHash, incomingHash))
                {
                    matchingToken = candidate;
                    break;
                }
            }

            if (matchingToken == null)
            {
                throw new InvalidSetupTokenException();
            }

            // Invalidate all tokens
            foreach (var token in activeTokens)
            {
                token.UsedAt = now;
            }

            // Create or update singleton system_setup record
            var systemSetup = await _dbContext.SystemSetup.FirstOrDefaultAsync(s => s.Id == 1, cancellationToken);
            if (systemSetup != null && systemSetup.IsCompleted)
            {
                throw new SetupAlreadyCompletedException();
            }

            if (systemSetup == null)
            {
                systemSetup = new SystemSetupRecord
                {
                    Id = 1,
                    IsCompleted = true,
                    CompletedAt = now,
                };
                _dbContext.SystemSetup.Add(systemSetup);
            }
            else
            {
                systemSetup.IsCompleted = true;
                systemSetup.CompletedAt = now;
            }

            // Create Organization
            var organization = new OrganizationRecord
            {
                Id = Guid.NewGuid(),
                Name = command.OrganizationName.Trim(),
                Slug = command.OrganizationSlug.Trim().ToLowerInvariant(),
                SetupCompletedAt = now,
                CreatedAt = now,
                UpdatedAt = now,
            };
            _dbContext.Organizations.Add(organization);

            // Create User (Administrator)
            var passwordHash = _passwordHasher.HashPassword(passwordVo.Value);
            var user = new UserRecord
            {
                Id = Guid.NewGuid(),
                Email = command.Email.Trim(),
                NormalizedEmail = command.Email.Trim().ToUpperInvariant(),
                PasswordHash = passwordHash,
                DisplayName = command.DisplayName.Trim(),
                EmailConfirmed = true,
                CreatedAt = now,
                UpdatedAt = now,
            };
            _dbContext.Users.Add(user);

            // Create Membership (Owner)
            var membership = new MembershipRecord
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                UserId = user.Id,
                Role = "owner",
                CreatedAt = now,
                UpdatedAt = now,
            };
            _dbContext.Memberships.Add(membership);

            systemSetup.OrganizationId = organization.Id;
            systemSetup.AdministratorId = user.Id;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsSingletonOrUniqueViolation(ex))
            {
                throw new ConcurrencySetupConflictException();
            }

            TryDeleteTokenFile();

            return new SetupResult(organization.Id, user.Id, organization.Slug);
        });
    }

    private static bool IsSingletonOrUniqueViolation(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        return msg.Contains("CK_system_setup_singleton", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("unique", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("23505", StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryWriteRestrictedTokenFile(string token)
    {
        try
        {
            var targetDir = Directory.Exists("/var/lib/marsa") ? "/var/lib/marsa" : ".marsa";
            Directory.CreateDirectory(targetDir);
            var filePath = Path.Combine(targetDir, "setup-token");
            File.WriteAllText(filePath, token);

            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                File.SetUnixFileMode(filePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }

            return filePath;
        }
        catch
        {
            return null;
        }
    }

    private static bool TokenFileExists()
    {
        return File.Exists("/var/lib/marsa/setup-token") || File.Exists(".marsa/setup-token");
    }

    private static void TryDeleteTokenFile()
    {
        try
        {
            var paths = new[] { "/var/lib/marsa/setup-token", ".marsa/setup-token" };
            foreach (var p in paths)
            {
                if (File.Exists(p))
                {
                    File.Delete(p);
                }
            }
        }
        catch
        {
            // Ignore file deletion errors
        }
    }
}
