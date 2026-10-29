using System.Security.Claims;
using Marsa.Application.Authentication;

namespace Marsa.Server.Middleware;

public sealed class SessionAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthenticationService authService)
    {
        var sessionToken = context.Request.Cookies["__Host-marsa_session"]
            ?? context.Request.Cookies["marsa_session"];

        if (!string.IsNullOrWhiteSpace(sessionToken))
        {
            var result = await authService.ValidateSessionAsync(sessionToken, context.RequestAborted);
            if (result.IsValid && result.User != null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
                    new(ClaimTypes.Name, result.User.DisplayName),
                    new(ClaimTypes.Email, result.User.Email),
                };

                if (result.User.Role != null)
                {
                    claims.Add(new(ClaimTypes.Role, result.User.Role));
                }

                if (result.User.OrganizationId != null)
                {
                    claims.Add(new("organization_id", result.User.OrganizationId.Value.ToString()));
                }

                var identity = new ClaimsIdentity(claims, "MarsaSession");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        await _next(context);
    }
}
