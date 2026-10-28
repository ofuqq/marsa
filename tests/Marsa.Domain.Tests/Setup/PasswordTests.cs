using Marsa.Domain.Setup;
using Xunit;

namespace Marsa.Domain.Tests.Setup;

public sealed class PasswordTests
{
    [Fact]
    public void Constructor_WithValidPassword_SetsValue()
    {
        var password = new Password("SecurePassword123!");

        Assert.Equal("SecurePassword123!", password.Value);
    }

    [Theory]
    [InlineData("12345678901")] // 11 characters
    [InlineData("short")]
    [InlineData("a")]
    public void Constructor_PasswordShorterThan12Characters_ThrowsArgumentException(string shortPassword)
    {
        var ex = Assert.Throws<ArgumentException>(() => new Password(shortPassword));
        Assert.Contains("at least 12 characters", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyOrWhitespace_ThrowsArgumentException(string? invalid)
    {
        Assert.Throws<ArgumentException>(() => new Password(invalid!));
    }

    [Fact]
    public void Constructor_PasswordLongerThan128Characters_ThrowsArgumentException()
    {
        var tooLong = new string('a', 129);

        var ex = Assert.Throws<ArgumentException>(() => new Password(tooLong));
        Assert.Contains("must not exceed 128 characters", ex.Message);
    }

    [Fact]
    public void ToString_MasksPasswordValue()
    {
        var password = new Password("SuperSecretPassword999!");

        var output = password.ToString();

        Assert.Equal("********", output);
        Assert.DoesNotContain("SuperSecretPassword999!", output);
    }
}
