using System.Text.RegularExpressions;
using Marsa.Application.Setup;
using Microsoft.AspNetCore.Mvc;

namespace Marsa.Server.Endpoints;

public static partial class SetupEndpoints
{
    [GeneratedRegex("^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?$", RegexOptions.Compiled)]
    private static partial Regex SlugRegex();

    public static IEndpointRouteBuilder MapSetupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/system/setup")
            .RequireRateLimiting("setup");

        group.MapGet("", async (
            [FromServices] ISetupService setupService,
            CancellationToken cancellationToken) =>
        {
            if (await setupService.IsSetupCompletedAsync(cancellationToken))
            {
                // After setup is complete, every setup endpoint returns 404 and reveals nothing.
                return Results.NotFound();
            }

            return Results.Ok(new
            {
                required = true,
                tokenRequired = true,
            });
        });

        group.MapPost("", async (
            [FromBody] SetupRequest request,
            [FromServices] ISetupService setupService,
            CancellationToken cancellationToken) =>
        {
            if (await setupService.IsSetupCompletedAsync(cancellationToken))
            {
                // Mask completion with 404 to reveal nothing about previous state.
                return Results.NotFound();
            }

            if (request == null)
            {
                return Results.BadRequest(CreateValidationProblem("Request payload cannot be empty."));
            }

            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return Results.BadRequest(CreateValidationProblem("Setup token is required."));
            }

            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            {
                return Results.BadRequest(CreateValidationProblem("A valid email address is required."));
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 12)
            {
                return Results.BadRequest(CreateValidationProblem("Password must be at least 12 characters long."));
            }

            if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Length > 120)
            {
                return Results.BadRequest(CreateValidationProblem("Display name must be between 1 and 120 characters."));
            }

            if (string.IsNullOrWhiteSpace(request.OrganizationName) || request.OrganizationName.Length > 120)
            {
                return Results.BadRequest(CreateValidationProblem("Organization name must be between 1 and 120 characters."));
            }

            var slug = request.OrganizationSlug?.Trim().ToLowerInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(slug) || !SlugRegex().IsMatch(slug))
            {
                return Results.BadRequest(CreateValidationProblem("Organization slug must be a valid DNS label (lowercase alphanumeric and hyphens, up to 63 characters)."));
            }

            try
            {
                var command = new SetupCommand(
                    request.Token.Trim(),
                    request.Email.Trim(),
                    request.Password,
                    request.DisplayName.Trim(),
                    request.OrganizationName.Trim(),
                    slug);

                var result = await setupService.CompleteSetupAsync(command, cancellationToken);

                // Returns 201 with organization and administrator IDs; never returns passwords, tokens, or hashes.
                return Results.Created($"/organizations/{result.OrganizationSlug}", new
                {
                    organizationId = result.OrganizationId,
                    administratorId = result.AdministratorId,
                    organizationSlug = result.OrganizationSlug,
                });
            }
            catch (InvalidSetupTokenException)
            {
                return Results.Unauthorized();
            }
            catch (SetupAlreadyCompletedException)
            {
                return Results.NotFound();
            }
            catch (ConcurrencySetupConflictException)
            {
                return Results.NotFound();
            }
        });

        return app;
    }

    private static object CreateValidationProblem(string detail)
    {
        return new
        {
            type = "https://marsa.dev/problems/validation-error",
            title = "Validation failed.",
            status = StatusCodes.Status400BadRequest,
            detail,
        };
    }
}

public sealed record SetupRequest(
    string? Token,
    string? Email,
    string? Password,
    string? DisplayName,
    string? OrganizationName,
    string? OrganizationSlug
);
