using TeaPie.Environments;

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

        registry.RegisterEnvironment(environment);

        var succeed = registry.TryGetEnvironment(environmentName, out var foundEnvironment);

        Assert.True(succeed);
        Assert.Equal(environment, foundEnvironment);
    }
}
