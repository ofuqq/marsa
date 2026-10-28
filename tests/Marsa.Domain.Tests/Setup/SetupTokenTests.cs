using Marsa.Domain.Setup;
using Xunit;

namespace Marsa.Domain.Tests.Setup;

public sealed class SetupTokenTests
{
    [Fact]
    public void Generate_ProducesSecureTokenWithPrefixAndHighEntropy()
    {
        var (entity, plaintext) = SetupToken.Generate();

        Assert.NotNull(plaintext);
        Assert.StartsWith("marsa_st_", plaintext);
        Assert.True(plaintext.Length >= 40);
        Assert.NotNull(entity);
        Assert.Equal(32, entity.TokenHash.Length); // 256-bit SHA-256 hash
        Assert.False(entity.IsUsed);
        Assert.Null(entity.UsedAt);
        Assert.True(entity.ExpiresAt > entity.CreatedAt);

        // Ensure consecutive generations produce distinct random tokens
        var (_, plaintext2) = SetupToken.Generate();
        Assert.NotEqual(plaintext, plaintext2);
    }

    [Fact]
    public void HashToken_ProducesDeterministicSha256()
    {
        var hash1 = SetupToken.HashToken("sample-token-1");
        var hash2 = SetupToken.HashToken("sample-token-1");
        var hash3 = SetupToken.HashToken("sample-token-2");

        Assert.Equal(32, hash1.Length);
        Assert.Equal(hash1, hash2);
        Assert.NotEqual(hash1, hash3);
    }

    [Fact]
    public void Verify_ValidToken_ReturnsTrue()
    {
        var (entity, plaintext) = SetupToken.Generate();

        var isValid = entity.Verify(plaintext, DateTimeOffset.UtcNow);

        Assert.True(isValid);
    }

    [Fact]
    public void Verify_InvalidOrEmptyToken_ReturnsFalse()
    {
        var (entity, _) = SetupToken.Generate();

        Assert.False(entity.Verify("marsa_st_wrong_token_guess", DateTimeOffset.UtcNow));
        Assert.False(entity.Verify("", DateTimeOffset.UtcNow));
        Assert.False(entity.Verify("   ", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Verify_ExpiredToken_ReturnsFalse()
    {
        var (entity, plaintext) = SetupToken.Generate(TimeSpan.FromHours(1));
        var expiredTime = entity.ExpiresAt.AddSeconds(1);

        Assert.False(entity.Verify(plaintext, expiredTime));
    }

    [Fact]
    public void Verify_ConsumedToken_ReturnsFalse()
    {
        var (entity, plaintext) = SetupToken.Generate();
        var now = DateTimeOffset.UtcNow;
        entity.Consume(now);

        Assert.True(entity.IsUsed);
        Assert.Equal(now, entity.UsedAt);
        Assert.False(entity.Verify(plaintext, now));
    }

    [Fact]
    public void Consume_CalledTwice_ThrowsInvalidOperationException()
    {
        var (entity, _) = SetupToken.Generate();
        var now = DateTimeOffset.UtcNow;
        entity.Consume(now);

        Assert.Throws<InvalidOperationException>(() => entity.Consume(now));
    }
}
