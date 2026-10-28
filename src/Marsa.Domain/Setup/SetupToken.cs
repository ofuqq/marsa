using System.Security.Cryptography;

namespace Marsa.Domain.Setup;

/// <summary>
/// A cryptographically secure, single-use token required to bootstrap the first administrator.
/// </summary>
public sealed class SetupToken
{
    private const string TokenPrefix = "marsa_st_";
    private const int RandomBytesCount = 32; // 256 bits of entropy

    public Guid Id { get; }
    public byte[] TokenHash { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset ExpiresAt { get; }
    public DateTimeOffset? UsedAt { get; private set; }

    public SetupToken(Guid id, byte[] tokenHash, DateTimeOffset createdAt, DateTimeOffset expiresAt, DateTimeOffset? usedAt = null)
    {
        Id = id;
        TokenHash = tokenHash ?? throw new ArgumentNullException(nameof(tokenHash));
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        UsedAt = usedAt;
    }

    /// <summary>
    /// Generates a new cryptographically secure setup token and returns the entity along with the plaintext token.
    /// </summary>
    public static (SetupToken TokenEntity, string PlaintextToken) Generate(TimeSpan? lifetime = null)
    {
        var randomBytes = new byte[RandomBytesCount];
        RandomNumberGenerator.Fill(randomBytes);

        var base64 = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        var plaintext = $"{TokenPrefix}{base64}";
        var hash = HashToken(plaintext);
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.Add(lifetime ?? TimeSpan.FromHours(24));

        var entity = new SetupToken(Guid.NewGuid(), hash, now, expiresAt);
        return (entity, plaintext);
    }

    /// <summary>
    /// Verifies the incoming plaintext token against the stored hash in constant time.
    /// </summary>
    public bool Verify(string plaintextToken, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(plaintextToken) || IsUsed || IsExpired(now))
        {
            return false;
        }

        var incomingHash = HashToken(plaintextToken);
        return CryptographicOperations.FixedTimeEquals(TokenHash, incomingHash);
    }

    public void Consume(DateTimeOffset now)
    {
        if (IsUsed)
        {
            throw new InvalidOperationException("Setup token has already been consumed.");
        }

        UsedAt = now;
    }

    public bool IsUsed => UsedAt.HasValue;

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    public static byte[] HashToken(string plaintextToken)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(plaintextToken);
        return SHA256.HashData(bytes);
    }
}
