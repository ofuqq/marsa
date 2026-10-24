using System.ComponentModel.DataAnnotations;
using Marsa.Server.Configuration;
using Xunit;

namespace Marsa.Application.Tests;

public class OptionsValidationTests
{
    [Fact]
    public void MarsaOptionsRequiresDatabaseConnectionString()
    {
        var options = new MarsaOptions();
        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(MarsaOptions.DatabaseConnectionString)));
    }
}
