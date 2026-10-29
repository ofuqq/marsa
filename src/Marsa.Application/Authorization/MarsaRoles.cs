namespace Marsa.Application.Authorization;

public static class MarsaRoles
{
    public const string Owner = "owner";
    public const string Admin = "admin";
    public const string Developer = "developer";
    public const string Viewer = "viewer";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Owner,
        Admin,
        Developer,
        Viewer,
    };
}

