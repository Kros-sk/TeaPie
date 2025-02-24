using TeaPie.Environments;
using static Xunit.Assert;

namespace TeaPie.Tests.Environments;

public class EnvironmentsRegistryShould
{
    [Fact]
    public void RegisterAndThenGetEnvironmentWithoutAnyProblems()
    {
        const string environmentName = "$shared";
        var registry = new EnvironmentsRegistry();
        var environment = new global::TeaPie.Environments.Environment(environmentName,
            new Dictionary<string, object?>() { { "abc", "efg" }, { "hij", 654 }, { "klm", null } });

        registry.Register(environment.Name, environment);

        True(registry.IsRegistered(environmentName));

        var foundEnvironment = registry.Get(environmentName);
        Equal(environment, foundEnvironment);
    }
}
