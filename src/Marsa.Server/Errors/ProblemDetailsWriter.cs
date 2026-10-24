using System.Text.Json;

namespace Marsa.Server.Errors;

public static class ProblemDetailsWriter
{
    public static async Task WriteAsync(HttpContext context, Exception exception)
    {
        var problem = new
        {
            type = "https://marsa.dev/problems/unhandled-error",
            title = "An unexpected error occurred.",
            status = StatusCodes.Status500InternalServerError,
            detail = "The request failed. No secrets or stack trace are shown.",
            code = "server.unhandled_error",
            traceId = context.TraceIdentifier,
        };

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(context.Response.Body, problem);
    }
}
