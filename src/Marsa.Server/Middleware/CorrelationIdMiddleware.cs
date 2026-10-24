namespace Marsa.Server.Middleware;

/// <summary>
/// Preserves a caller-supplied correlation ID when it is safe to use and creates
/// one when it is missing or malformed.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Request-Id";
    private const int MaximumLength = 128;

    public async Task InvokeAsync(HttpContext context)
    {
        var supplied = context.Request.Headers[HeaderName].FirstOrDefault();
        var correlationId = IsValid(supplied) ? supplied! : Guid.NewGuid().ToString("N");

        context.Request.Headers[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;
        context.TraceIdentifier = correlationId;

        await next(context);
    }

    private static bool IsValid(string? value)
        => !string.IsNullOrWhiteSpace(value)
            && value.Length <= MaximumLength
            && value.All(static c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.');
}
