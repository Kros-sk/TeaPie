using Microsoft.Extensions.DependencyInjection;
using TeaPie.Environments;
using TeaPie.Logging;
using TeaPie.Pipelines;
using TeaPie.StructureExploration;
using TeaPie.Variables;

namespace TeaPie.Tests.Environments;

[Collection(nameof(NonParallelCollection))]
public class InitializeEnvironmentStepShould
{
    private static readonly string _collectionPath = Path.Combine(Directory.GetCurrentDirectory(), "Demo", "Environments");

    [Fact]
    public async Task RegisterAllAvailableEnvironmentsCollection()
    {
        PrepareServices(out var pipeline, out var provider, out var _, out var environmentsRegistry, out var appContextBuilder);
        await RunApplicationPipeline(pipeline, provider, appContextBuilder);

        CheckExistenceOfEnvironment(Constants.DefaultEnvironmentName, environmentsRegistry);
        CheckExistenceOfEnvironment("test-lab", environmentsRegistry);
        CheckExistenceOfEnvironment("empty", environmentsRegistry);
        CheckExistenceOfEnvironment("allKind", environmentsRegistry);
    }

    [Fact]
    public async Task SetDefaultEnvironmentIfNoEnvironmentIsGiven()
    {
        PrepareServices(out var pipeline, out var provider, out var variables, out var environmentsRegistry, out var appContextBuilder);
        await RunApplicationPipeline(pipeline, provider, appContextBuilder);

        CheckExistenceOfEnvironment(Constants.DefaultEnvironmentName, environmentsRegistry);
        Assert.Equal("/customers", variables.GlobalVariables.Get<string>("ApiCustomersSection"));
        Assert.Equal("/cars", variables.GlobalVariables.Get<string>("ApiCarsSection"));
        Assert.Equal("/rental", variables.GlobalVariables.Get<string>("ApiCarRentalSection"));
        Assert.Equal("/customers", variables.EnvironmentVariables.Get<string>("ApiCustomersSection"));
        Assert.Equal("/cars", variables.EnvironmentVariables.Get<string>("ApiCarsSection"));
        Assert.Equal("/rental", variables.EnvironmentVariables.Get<string>("ApiCarRentalSection"));
        Assert.Equal("/customers", variables.GetVariable<string>("ApiCustomersSection"));
        Assert.Equal("/cars", variables.GetVariable<string>("ApiCarsSection"));
        Assert.Equal("/rental", variables.GetVariable<string>("ApiCarRentalSection"));
    }

    [Fact]
    public async Task SetGivenEnvironment()
    {
        PrepareServices(out var pipeline, out var provider, out var variables, out var environmentsRegistry, out var appContextBuilder);
        await RunApplicationPipeline(pipeline, provider, appContextBuilder, "test-lab");

        CheckExistenceOfEnvironment("test-lab", environmentsRegistry);
        Assert.Equal("http://localhost:3001", variables.EnvironmentVariables.Get<string>("ApiBaseUrl"));
        Assert.Equal("stringValue", variables.EnvironmentVariables.Get<string>("StringVar"));
        Assert.True(variables.EnvironmentVariables.Get<bool>("BooleanVar"));
        Assert.Equal(25.6m, variables.EnvironmentVariables.Get<decimal>("DoubleVar"));
        Assert.Equal(199, variables.EnvironmentVariables.Get<int>("IntVar"));
        Assert.Equal(["one", "two", "three"], variables.EnvironmentVariables.Get<List<object>>("ListVar"));
    }

    [Fact]
    public async Task LoadDefaultEnvironmentVariablesEvenIfAnotherEnvironmentIsGiven()
    {
        PrepareServices(out var pipeline, out var provider, out var variables, out var environmentsRegistry, out var appContextBuilder);
        await RunApplicationPipeline(pipeline, provider, appContextBuilder, "test-lab");

        CheckExistenceOfEnvironment("test-lab", environmentsRegistry);
        CheckExistenceOfEnvironment(Constants.DefaultEnvironmentName, environmentsRegistry);
        Assert.Equal("/customers", variables.GlobalVariables.Get<string>("ApiCustomersSection"));
        Assert.Equal("/cars", variables.GlobalVariables.Get<string>("ApiCarsSection"));
        Assert.Equal("/rental", variables.GlobalVariables.Get<string>("ApiCarRentalSection"));
        Assert.Equal("/customers", variables.GetVariable<string>("ApiCustomersSection"));
        Assert.Equal("/cars", variables.GetVariable<string>("ApiCarsSection"));
        Assert.Equal("/rental", variables.GetVariable<string>("ApiCarRentalSection"));
    }

    [Fact]
    public async Task OverrideVariablesFromDefaultEnvironmentWithSameNameVariablesFromGivenEnvironment()
    {
        PrepareServices(out var pipeline, out var provider, out var variables, out var environmentsRegistry, out var appContextBuilder);
        await RunApplicationPipeline(pipeline, provider, appContextBuilder, "test-lab");

        CheckExistenceOfEnvironment("test-lab", environmentsRegistry);
        CheckExistenceOfEnvironment(Constants.DefaultEnvironmentName, environmentsRegistry);
        Assert.Equal("http://localhost:3001", variables.GetVariable<string>("ApiBaseUrl"));
    }

    private static async Task RunApplicationPipeline(
        IPipeline pipeline,
        IServiceProvider provider,
        ApplicationContextBuilder appContextBuilder,
        string environmentName = "")
    {
        var structureExplorationStep = provider.GetStep<ExploreStructureStep>();
        var step = provider.GetStep<InitializeEnvironmentsStep>();

        pipeline.AddSteps(structureExplorationStep);
        pipeline.AddSteps(step);

        if (!environmentName.Equals(string.Empty))
        {
            appContextBuilder = appContextBuilder.WithEnvironment(environmentName);
        }

        var appContext = appContextBuilder.Build();

        await pipeline.Run(appContext, CancellationToken.None);
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

    private static void CheckExistenceOfEnvironment(string environmentName, IEnvironmentsRegistry environmentsRegistry)
    {
        var hasEnv = environmentsRegistry.TryGetEnvironment(environmentName, out var foundEnv);

        Assert.True(hasEnv);
        Assert.NotNull(foundEnv);
    }
}
