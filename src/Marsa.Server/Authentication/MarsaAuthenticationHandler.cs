using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Marsa.Server.Authentication;

public sealed class MarsaAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "MarsaAuth";

    public MarsaAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Context.User.Identity?.IsAuthenticated == true)
        {
            var ticket = new AuthenticationTicket(Context.User, SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.NoResult());
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (IsApiRequest(Request))
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            Response.ContentType = "application/problem+json";

            var problem = new
            {
                type = "https://httpstatuses.com/401",
                title = "Unauthorized",
                status = StatusCodes.Status401Unauthorized,
                detail = "Authentication is required to access this resource.",
                traceId = Context.TraceIdentifier,
            };

            await JsonSerializer.SerializeAsync(Response.Body, problem);
            return;
        }

        var returnUrl = Request.Path + Request.QueryString;
        var redirectUrl = string.IsNullOrWhiteSpace(returnUrl) || returnUrl == "/"
            ? "/login"
            : $"/login?returnUrl={Uri.EscapeDataString(returnUrl)}";

        Response.Redirect(redirectUrl);
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;

        if (IsApiRequest(Request))
        {
            Response.ContentType = "application/problem+json";

            var problem = new
            {
                type = "https://httpstatuses.com/403",
                title = "Forbidden",
                status = StatusCodes.Status403Forbidden,
                detail = "You do not have permission to access this resource.",
                traceId = Context.TraceIdentifier,
            };

            await JsonSerializer.SerializeAsync(Response.Body, problem);
            return;
        }

        Response.ContentType = "text/html; charset=utf-8";
        await Response.WriteAsync(
            "<!DOCTYPE html><html lang=\"en\"><head><title>Forbidden</title></head><body><h1>403 Forbidden</h1><p>You do not have permission to access this view.</p><p><a href=\"/\">Return to dashboard</a></p></body></html>");
    }

    private static bool IsApiRequest(HttpRequest request)
    {
        if (request.Path.StartsWithSegments("/api"))
        {
            return true;
        }

        if (request.Headers.Accept.Any(h => h != null && h.Contains("application/json", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (request.Headers.ContainsKey("X-Requested-With"))
        {
            return true;
        }

        return false;
    }
}

