using Marsa.Contracts;
using Marsa.Domain.Common;
using NetArchTest.Rules;
using Xunit;

namespace Marsa.Domain.Tests;

public class ArchitectureTests
{
    [Fact]
    public void DomainDoesNotReferenceOtherMarsaProjects()
    {
        var result = Types.InAssembly(typeof(DomainAssemblyMarker).Assembly)
            .ShouldNot().HaveDependencyOnAny(
                "Marsa.Contracts",
                "Marsa.Application",
                "Marsa.Infrastructure",
                "Marsa.Server",
                "Marsa.Cli")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(Environment.NewLine, result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void ContractsDoesNotReferenceDomainOrLaterProjects()
    {
        var result = Types.InAssembly(typeof(ContractAssemblyMarker).Assembly)
            .ShouldNot().HaveDependencyOnAny(
                "Marsa.Domain",
                "Marsa.Application",
                "Marsa.Infrastructure",
                "Marsa.Server",
                "Marsa.Cli")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(Environment.NewLine, result.FailingTypeNames ?? Array.Empty<string>()));
    }
}
