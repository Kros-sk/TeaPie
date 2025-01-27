using TeaPie.Environments;

namespace TeaPie.Tests.Environments;

public class InitializeEnvironmentStepShould
{
    [Fact]
    public async Task RegisterAllAvailableEnvironmentsCollection()
    {
        var variables = new global::TeaPie.Variables.Variables();
        var environmentsRegistry = new EnvironmentsRegistry();
        var pipeline = new ApplicationPipeline();

        var appContext = new ApplicationContextBuilder()
            .WithEnvironmentFilePath("./Demo/Environments/environments-env.json").Build();

        var step = new InitializeEnvironmentsStep(pipeline, variables, environmentsRegistry);

        pipeline.AddSteps(step);

        await pipeline.Run(appContext, CancellationToken.None);

        var hasDefaultEnv = environmentsRegistry.TryGetEnvironment(Constants.DefaultEnvironmentName, out var defaultEnv);
        var hasTestLabEnv = environmentsRegistry.TryGetEnvironment("test-lab", out var testLabEnv);
        var hasEmptyEnv = environmentsRegistry.TryGetEnvironment("empty", out var emptyEnv);

        Assert.True(hasDefaultEnv);
        Assert.NotNull(defaultEnv);
        Assert.True(hasTestLabEnv);
        Assert.NotNull(testLabEnv);
        Assert.True(hasEmptyEnv);
        Assert.NotNull(emptyEnv);
    }
}
