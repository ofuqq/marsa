namespace Marsa.Application.Authentication;

public interface IAuthenticationService
{
    Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);
    Task<SessionValidationResult> ValidateSessionAsync(string sessionToken, CancellationToken cancellationToken = default);
    Task RevokeSessionAsync(string sessionToken, CancellationToken cancellationToken = default);
}
