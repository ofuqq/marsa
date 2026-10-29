using Marsa.Application.Authorization;
using Xunit;

namespace Marsa.Application.Tests.Authorization;

public sealed class MarsaRoleHierarchyTests
{
    [Theory]
    [InlineData(MarsaRoles.Owner, MarsaRoles.Owner, true)]
    [InlineData(MarsaRoles.Owner, MarsaRoles.Admin, true)]
    [InlineData(MarsaRoles.Owner, MarsaRoles.Developer, true)]
    [InlineData(MarsaRoles.Owner, MarsaRoles.Viewer, true)]
    [InlineData(MarsaRoles.Admin, MarsaRoles.Owner, false)]
    [InlineData(MarsaRoles.Admin, MarsaRoles.Admin, true)]
    [InlineData(MarsaRoles.Admin, MarsaRoles.Developer, true)]
    [InlineData(MarsaRoles.Admin, MarsaRoles.Viewer, true)]
    [InlineData(MarsaRoles.Developer, MarsaRoles.Admin, false)]
    [InlineData(MarsaRoles.Developer, MarsaRoles.Developer, true)]
    [InlineData(MarsaRoles.Developer, MarsaRoles.Viewer, true)]
    [InlineData(MarsaRoles.Viewer, MarsaRoles.Developer, false)]
    [InlineData(MarsaRoles.Viewer, MarsaRoles.Viewer, true)]
    [InlineData("unknown", MarsaRoles.Viewer, false)]
    [InlineData(null, MarsaRoles.Viewer, false)]
    public void SatisfiesRole_CorrectlyEvaluatesRoleHierarchy(string? userRole, string minimumRole, bool expected)
    {
        var actual = MarsaRoleHierarchy.SatisfiesRole(userRole, minimumRole);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetSatisfyingRoles_ForAdmin_ReturnsOwnerAndAdmin()
    {
        var roles = MarsaRoleHierarchy.GetSatisfyingRoles(MarsaRoles.Admin);
        Assert.Contains(MarsaRoles.Owner, roles);
        Assert.Contains(MarsaRoles.Admin, roles);
        Assert.DoesNotContain(MarsaRoles.Developer, roles);
        Assert.DoesNotContain(MarsaRoles.Viewer, roles);
    }
}

