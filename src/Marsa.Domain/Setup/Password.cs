namespace Marsa.Domain.Setup;

/// <summary>
/// Password value object ensuring minimum length and basic hygiene rules before hashing.
/// </summary>
public sealed record Password
{
    public const int MinimumLength = 12;
    public const int MaximumLength = 128;

    public string Value { get; }

    public Password(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Password cannot be empty.", nameof(value));
        }

        if (value.Length < MinimumLength)
        {
            throw new ArgumentException($"Password must be at least {MinimumLength} characters long.", nameof(value));
        }

        if (value.Length > MaximumLength)
        {
            throw new ArgumentException($"Password must not exceed {MaximumLength} characters.", nameof(value));
        }

        Value = value;
    }

    public override string ToString() => "********";
}
