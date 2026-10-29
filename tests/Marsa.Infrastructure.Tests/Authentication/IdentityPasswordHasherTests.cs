using Marsa.Infrastructure.Authentication;
using Xunit;

namespace Marsa.Infrastructure.Tests.Authentication;

public sealed class IdentityPasswordHasherTests
{
    [Fact]
    public void HashPassword_ProducesSecureNonPlaintextHash()
    {
        var hasher = new IdentityPasswordHasher();
        var plaintext = "MySecurePass1234!";

        var hash = hasher.HashPassword(plaintext);

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.DoesNotContain(plaintext, hash);

        // Successive calls produce different salted hashes
        var hash2 = hasher.HashPassword(plaintext);
        Assert.NotEqual(hash, hash2);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var hasher = new IdentityPasswordHasher();
        var plaintext = "MySecurePass1234!";
        var hash = hasher.HashPassword(plaintext);

        var result = hasher.VerifyPassword(hash, plaintext);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hasher = new IdentityPasswordHasher();
        var hash = hasher.HashPassword("MySecurePass1234!");

        var result = hasher.VerifyPassword(hash, "WrongPassword123!");

        Assert.False(result);
    }

    [Theory]
    [InlineData("", "pass")]
    [InlineData("hash", "")]
    [InlineData(null, "pass")]
    [InlineData("hash", null)]
    public void VerifyPassword_NullOrEmpty_ReturnsFalse(string? hash, string? password)
    {
        var hasher = new IdentityPasswordHasher();

        Assert.False(hasher.VerifyPassword(hash!, password!));
    }

    [Fact]
    public void VerifyDummyPassword_AlwaysReturnsFalse()
    {
        var hasher = new IdentityPasswordHasher();

        var result = hasher.VerifyDummyPassword("AnyPassword123!");

        Assert.False(result);
    }

    [Fact]
    public void VerifyPasswordWithRehash_WhenValid_ReturnsValidTrue()
    {
        var hasher = new IdentityPasswordHasher();
        var hash = hasher.HashPassword("MySecurePass1234!");

        var (isValid, _) = hasher.VerifyPasswordWithRehash(hash, "MySecurePass1234!");

        Assert.True(isValid);
    }
}
