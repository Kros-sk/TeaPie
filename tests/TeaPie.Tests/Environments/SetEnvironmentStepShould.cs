using Microsoft.Extensions.DependencyInjection;
using TeaPie.Application;
using TeaPie.Environments;
using TeaPie.Pipelines;
using TeaPie.StructureExploration;
using TeaPie.Variables;

namespace TeaPie.Tests.Environments;

[Collection(nameof(NonParallelCollection))]
public class SetEnvironmentStepShould
{
    private static readonly string _collectionPath = Path.Combine(Directory.GetCurrentDirectory(), "Demo", "Environments");
    public readonly Guid Guid = Guid.Parse("a68a15f4-f7da-4075-b779-a2e42e1c460c");

    [Fact]
    public async Task SetAllOfItsVariablesCorrectlyToEnvironmentCollection()
    {
        PrepareServices(out var pipeline, out var provider, out var variables, out var environmentsRegistry, out var appContextBuilder);
        var environment = GetEnvironment();

        var appContext = appContextBuilder.WithEnvironment("allKind").Build();
        pipeline.AddSteps(provider.GetStep<ExploreStructureStep>());
        pipeline.AddSteps(provider.GetStep<InitializeEnvironmentsStep>());

        await pipeline.Run(appContext, CancellationToken.None);

        Assert.True(variables.EnvironmentVariables.Get<bool>("boolVariable"));
        Assert.Equal(987, variables.EnvironmentVariables.Get<int>("intVariable"));
        Assert.Equal(9223372036854775807L, variables.EnvironmentVariables.Get<long>("longVariable"));
        Assert.Equal(65.4m, variables.EnvironmentVariables.Get<decimal>("decimalVariable"));
        Assert.Equal(DateTimeOffset.Parse("2025-01-27T12:34:56+01:00"),
            variables.EnvironmentVariables.Get<DateTimeOffset>("dateTimeOffsetVariable"));
        Assert.Equal(Guid, variables.EnvironmentVariables.Get<Guid>("guidVariable"));
        Assert.Equal("abc", variables.EnvironmentVariables.Get<string>("stringVariable"));
        Assert.Equal([1.2m, 3.4m, 5.6m],
            variables.EnvironmentVariables.Get<List<object>>("arrayVariable"));
        Assert.Null(variables.EnvironmentVariables.Get<object?>("nullVariable"));
    }

    private static void PrepareServices(
        out IPipeline pipeline,
        out IServiceProvider provider,
        out IVariables variables,
        out IEnvironmentsRegistry environmentsRegistry,
        out ApplicationContextBuilder appContextBuilder)
    {
        var services = new ServiceCollection();
        services.AddScoped<ExploreStructureStep>();
        services.AddScoped<InitializeEnvironmentsStep>();
        services.AddScoped<SetEnvironmentStep>();
        services.AddSingleton<IVariables, global::TeaPie.Variables.Variables>();
        services.AddSingleton<IEnvironmentsRegistry, EnvironmentsRegistry>();
        services.AddSingleton<IPipeline, ApplicationPipeline>();
        services.AddSingleton<IStructureExplorer, StructureExplorer>();
        services.AddLogging();

        provider = services.BuildServiceProvider();

        pipeline = provider.GetRequiredService<IPipeline>();
        variables = provider.GetRequiredService<IVariables>();
        environmentsRegistry = provider.GetRequiredService<IEnvironmentsRegistry>();

        appContextBuilder = new ApplicationContextBuilder()
            .WithPath(_collectionPath)
            .WithServiceProvider(provider);
    }

    private global::TeaPie.Environments.Environment GetEnvironment()
        => new(Constants.DefaultEnvironmentName, new()
        {
            { "boolVariable", true },
            { "intVariable", 987 },
            { "longVariable", 9223372036854775807L },
            { "decimalVariable", 65.4m },
            { "dateTimeOffsetVariable", DateTimeOffset.Parse("2025-01-27T12:34:56+01:00") },
            { "guidVariable", Guid },
            { "stringVariable", "abc" },
            { "arrayVariable", new List<decimal>(){ 1.2m, 3.4m, 5.6m } },
            { "nullVariable", null }
        });
}
