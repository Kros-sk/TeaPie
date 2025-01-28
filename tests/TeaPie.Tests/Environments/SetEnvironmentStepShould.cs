using TeaPie.Environments;

namespace TeaPie.Tests.Environments;

public class SetEnvironmentStepShould
{
    public readonly string GuidValue = Guid.NewGuid().ToString();

    [Fact]
    public async Task SetAllOfItsVariablesToEnvironmentCollection()
    {
        var environment = GetEnvironment();
        var variables = new global::TeaPie.Variables.Variables();
        var environmentsRegistry = new EnvironmentsRegistry();

        environmentsRegistry.RegisterEnvironment(environment);

        var appContext = new ApplicationContextBuilder().Build();

        var step = new SetEnvironmentStep(variables, environmentsRegistry);

        await step.Execute(appContext, CancellationToken.None);

        Assert.True(variables.EnvironmentVariables.Get<bool>("boolVariable"));
        Assert.Equal(987, variables.EnvironmentVariables.Get<int>("intVariable"));
        Assert.Equal(9223372036854775807L, variables.EnvironmentVariables.Get<long>("longVariable"));
        Assert.Equal(65.4m, variables.EnvironmentVariables.Get<decimal>("decimalVariable"));
        Assert.Equal(DateTimeOffset.Parse("2025-01-27T12:34:56+01:00"),
            variables.EnvironmentVariables.Get<DateTimeOffset>("dateTimeOffsetVariable"));
        Assert.Equal(GuidValue, variables.EnvironmentVariables.Get<string>("guidVariable"));
        Assert.Equal("abc", variables.EnvironmentVariables.Get<string>("stringVariable"));
        Assert.Equal([1.2m, 3.4m, 5.6m],
            variables.EnvironmentVariables.Get<List<decimal>>("arrayVariable"));
        Assert.Null(variables.EnvironmentVariables.Get<object?>("nullVariable"));
    }

    private global::TeaPie.Environments.Environment GetEnvironment()
        => new(Constants.DefaultEnvironmentName, new()
        {
            { "boolVariable", true },
            { "intVariable", 987 },
            { "longVariable", 9223372036854775807L },
            { "decimalVariable", 65.4m },
            { "dateTimeOffsetVariable", DateTimeOffset.Parse("2025-01-27T12:34:56+01:00") },
            { "guidVariable", GuidValue },
            { "stringVariable", "abc" },
            { "arrayVariable", new List<decimal>(){ 1.2m, 3.4m, 5.6m } },
            { "nullVariable", null }
        });
}
