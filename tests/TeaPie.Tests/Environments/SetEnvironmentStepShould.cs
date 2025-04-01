using Microsoft.Extensions.DependencyInjection;
using TeaPie.Environments;
using TeaPie.Pipelines;
using TeaPie.StructureExploration;
using TeaPie.StructureExploration.Paths;
using TeaPie.Variables;
using static Xunit.Assert;

namespace TeaPie.Tests.Environments;

[Collection(nameof(NonParallelCollection))]
public class SetEnvironmentStepShould
{
    private static readonly string _collectionPath = Path.Combine(Directory.GetCurrentDirectory(), "Demo", "Environments");
    public readonly Guid Guid = Guid.Parse("a68a15f4-f7da-4075-b779-a2e42e1c460c");

    [Fact]
    public async Task SetAllOfItsVariablesCorrectlyToEnvironmentCollection()
    {
        PrepareServices(out var pipeline, out var provider, out var variables, out var appContextBuilder);

        var appContext = appContextBuilder.WithEnvironment("allKind").Build();
        pipeline.AddSteps(provider.GetStep<ExploreStructureStep>());
        pipeline.AddSteps(provider.GetStep<InitializeEnvironmentsStep>());

        await pipeline.Run(appContext, CancellationToken.None);

        True(variables.EnvironmentVariables.Get<bool>("boolVariable"));
        Equal(987, variables.EnvironmentVariables.Get<int>("intVariable"));
        Equal(9223372036854775807L, variables.EnvironmentVariables.Get<long>("longVariable"));
        Equal(65.4m, variables.EnvironmentVariables.Get<decimal>("decimalVariable"));
        Equal(DateTimeOffset.Parse("2025-01-27T12:34:56+01:00"),
            variables.EnvironmentVariables.Get<DateTimeOffset>("dateTimeOffsetVariable"));
        Equal(Guid, variables.EnvironmentVariables.Get<Guid>("guidVariable"));
        Equal("abc", variables.EnvironmentVariables.Get<string>("stringVariable"));
        Equal([1.2m, 3.4m, 5.6m],
            variables.EnvironmentVariables.Get<List<object>>("arrayVariable"));
        Null(variables.EnvironmentVariables.Get<object?>("nullVariable"));
    }

    private static void PrepareServices(
        out IPipeline pipeline,
        out IServiceProvider provider,
        out IVariables variables,
        out ApplicationContextBuilder appContextBuilder)
    {
        var services = new ServiceCollection();
        services.AddTeaPie(true, () => { });

        provider = services.BuildServiceProvider();

        pipeline = provider.GetRequiredService<IPipeline>();
        variables = provider.GetRequiredService<IVariables>();

        var pathProvider = provider.GetRequiredService<IPathProvider>();
        pathProvider.UpdatePaths(_collectionPath, Constants.SystemTemporaryFolderPath);

        appContextBuilder = new ApplicationContextBuilder()
            .WithPath(_collectionPath)
            .WithServiceProvider(provider);
    }
}
