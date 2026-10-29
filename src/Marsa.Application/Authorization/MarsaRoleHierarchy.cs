namespace Marsa.Application.Authorization;

public static class MarsaRoleHierarchy
{
    private static readonly Dictionary<string, int> RoleRanks = new(StringComparer.OrdinalIgnoreCase)
    {
        [MarsaRoles.Owner] = 4,
        [MarsaRoles.Admin] = 3,
        [MarsaRoles.Developer] = 2,
        [MarsaRoles.Viewer] = 1,
    };

    public static bool SatisfiesRole(string? userRole, string minimumRole)
    {
        if (string.IsNullOrWhiteSpace(userRole) || string.IsNullOrWhiteSpace(minimumRole))
        {
            return false;
        }

        if (!RoleRanks.TryGetValue(userRole, out var userRank))
        {
            return false;
        }

        if (!RoleRanks.TryGetValue(minimumRole, out var requiredRank))
        {
            return false;
        }

        return userRank >= requiredRank;
    }

    public static string[] GetSatisfyingRoles(string minimumRole)
    {
        if (!RoleRanks.TryGetValue(minimumRole, out var requiredRank))
        {
            return Array.Empty<string>();
        }

        return RoleRanks
            .Where(pair => pair.Value >= requiredRank)
            .Select(pair => pair.Key)
            .ToArray();
    }
}

