namespace Marsa.Application.Authentication;

/// <summary>
/// Abstraction for secure password hashing using ASP.NET Core Identity defaults.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}
