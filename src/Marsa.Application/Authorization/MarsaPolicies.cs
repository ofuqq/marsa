namespace Marsa.Application.Authorization;

public static class MarsaPolicies
{
    public const string RequireAuthenticated = "RequireAuthenticated";
    public const string RequireOwner = "RequireOwner";
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireDeveloper = "RequireDeveloper";
    public const string RequireViewer = "RequireViewer";
}

