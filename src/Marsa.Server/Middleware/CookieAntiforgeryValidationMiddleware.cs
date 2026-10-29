using Microsoft.AspNetCore.Antiforgery;

namespace Marsa.Server.Middleware;

public sealed class CookieAntiforgeryValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAntiforgery _antiforgery;

    public CookieAntiforgeryValidationMiddleware(RequestDelegate next, IAntiforgery antiforgery)
    {
        _next = next;
        _antiforgery = antiforgery;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var isUnsafeMethod = HttpMethods.IsPost(method) ||
                             HttpMethods.IsPut(method) ||
                             HttpMethods.IsDelete(method) ||
                             HttpMethods.IsPatch(method);

        var hasBearer = context.Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
        var hasCookieAuth = context.Request.Cookies.ContainsKey("__Host-marsa_session") ||
                            context.Request.Cookies.ContainsKey("marsa_session");

        var path = context.Request.Path.Value ?? string.Empty;
        var isExempt = path.Equals("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase) ||
                       path.Equals("/system/setup", StringComparison.OrdinalIgnoreCase) ||
                       path.Equals("/api/v1/system/setup", StringComparison.OrdinalIgnoreCase);

        if (isUnsafeMethod && hasCookieAuth && !hasBearer && !isExempt)
        {
            try
            {
                await _antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://marsa.dev/problems/antiforgery-validation-failed",
                    title = "Antiforgery token validation failed.",
                    status = StatusCodes.Status400BadRequest,
                    detail = "A valid antiforgery token must be provided via the X-CSRF-TOKEN header or form body for cookie-authenticated requests.",
                });
                return;
            }
        }

        await _next(context);
    }
}
