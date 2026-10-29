using System.Security.Claims;
using Marsa.Application.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace Marsa.Server.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            [FromServices] IAuthenticationService authService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new
                {
                    type = "https://marsa.dev/problems/validation-error",
                    title = "Validation failed.",
                    status = StatusCodes.Status400BadRequest,
                    detail = "Email and password are required.",
                });
            }

            var command = new LoginCommand(request.Email.Trim(), request.Password);
            var result = await authService.LoginAsync(command, cancellationToken);

            if (result.IsLockedOut)
            {
                return Results.Json(new
                {
                    type = "https://marsa.dev/problems/account-locked",
                    title = "Account temporarily locked.",
                    status = StatusCodes.Status429TooManyRequests,
                    detail = result.ErrorMessage ?? "Account is temporarily locked due to repeated failed attempts.",
                }, statusCode: StatusCodes.Status429TooManyRequests);
            }

            if (!result.Succeeded || result.SessionToken == null || result.User == null)
            {
                return Results.Json(new
                {
                    type = "https://marsa.dev/problems/invalid-credentials",
                    title = "Invalid credentials.",
                    status = StatusCodes.Status401Unauthorized,
                    detail = "Invalid email or password.",
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            // Set session cookie
            var cookieName = httpContext.Request.IsHttps ? "__Host-marsa_session" : "marsa_session";
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddHours(24),
            };

            httpContext.Response.Cookies.Append(cookieName, result.SessionToken, cookieOptions);

            return Results.Ok(new
            {
                user = new
                {
                    id = result.User.Id,
                    email = result.User.Email,
                    displayName = result.User.DisplayName,
                    organizationId = result.User.OrganizationId,
                    role = result.User.Role,
                }
            });
        }).RequireRateLimiting("login").AllowAnonymous();

        group.MapPost("/logout", async (
            [FromServices] IAuthenticationService authService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var sessionToken = httpContext.Request.Cookies["__Host-marsa_session"]
                ?? httpContext.Request.Cookies["marsa_session"];

            if (!string.IsNullOrWhiteSpace(sessionToken))
            {
                await authService.RevokeSessionAsync(sessionToken, cancellationToken);
            }

            httpContext.Response.Cookies.Delete("__Host-marsa_session", new CookieOptions { Path = "/", Secure = true });
            httpContext.Response.Cookies.Delete("marsa_session", new CookieOptions { Path = "/" });

            return Results.NoContent();
        });

        group.MapGet("/me", (HttpContext httpContext) =>
        {
            if (httpContext.User.Identity?.IsAuthenticated != true)
            {
                return Results.Json(new
                {
                    type = "https://marsa.dev/problems/unauthorized",
                    title = "Unauthorized.",
                    status = StatusCodes.Status401Unauthorized,
                    detail = "Authentication is required to access this resource.",
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = httpContext.User.FindFirstValue(ClaimTypes.Email);
            var displayName = httpContext.User.FindFirstValue(ClaimTypes.Name);
            var role = httpContext.User.FindFirstValue(ClaimTypes.Role);
            var orgId = httpContext.User.FindFirstValue("organization_id");

            return Results.Ok(new
            {
                id = userId,
                email,
                displayName,
                organizationId = orgId != null ? Guid.Parse(orgId) : (Guid?)null,
                role,
            });
        });

        group.MapGet("/antiforgery", (IAntiforgery antiforgery, HttpContext httpContext) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(httpContext);
            return Results.Ok(new
            {
                token = tokens.RequestToken,
                headerName = "X-CSRF-TOKEN",
            });
        }).AllowAnonymous();

        return app;
    }
}

public sealed record LoginRequest(string? Email, string? Password);
