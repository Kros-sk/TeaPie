using Microsoft.Extensions.DependencyInjection;
using TeaPie.Environments;
using TeaPie.Logging;
using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.StructureExploration;
using TeaPie.Variables;
using static Xunit.Assert;

namespace TeaPie.Tests.Environments;

[Collection(nameof(NonParallelCollection))]
public class InitializeEnvironmentStepShould
{
    private static readonly string _collectionPath = Path.Combine(Directory.GetCurrentDirectory(), "Demo", "Environments");
    private static readonly string _collectionWithNestedEnvironmentFilePath = Path.Combine(
        _collectionPath,
        "CollectionWithNestedEnvironmentFile");

    private static readonly string _explicitEnvironmentFilePath = Path.Combine(
        _collectionWithNestedEnvironmentFilePath,
        "Folder2",
        "CollectionWithNestedEnvironmentFile-env.json");

    [Fact]
    public async Task FailIfGivenEnvironmentFileDoesntExists()
    {
        PrepareServices(out var pipeline, out var provider, out _, out _, out var appContextBuilder);

        var result = await RunApplicationPipeline(
            pipeline,
            provider,
            appContextBuilder,
            Constants.DefaultEnvironmentName,
            Path.Combine(_collectionPath, "NonExisting-env.json"));

        Equal(1, result);
    }

    [Fact]
    public async Task RunWithoutEnvironmentIfNoEnvironmentFileIsGivenOrFound()
    {
        PrepareServices(
            out var pipeline,
            out var provider,
            out _,
            out _,
            out var appContextBuilder,
            Path.Combine(_collectionPath, "CollectionWithoutEnvironment"));

        var result = await RunApplicationPipeline(pipeline, provider, appContextBuilder);

        Equal(0, result);
    }

    [Fact]
    public async Task FailIfImplicitEnvironmentFileDoesntContainSpecifiedEnvironment()
    {
        PrepareServices(out var pipeline, out var provider, out _, out var environmentsRegistry, out var appContextBuilder);

        var result = await RunApplicationPipeline(pipeline, provider, appContextBuilder, "non-existing-environment");

        Equal(1, result);

        var exists = environmentsRegistry.TryGetEnvironment("non-existing-environment", out var found);

        False(exists);
        Null(found);
    }

    [Fact]
    public async Task FailIfExplicitEnvironmentFileDoesntContainSpecifiedEnvironment()
    {
        PrepareServices(out var pipeline, out var provider, out _, out var environmentsRegistry, out var appContextBuilder);

        var result = await RunApplicationPipeline(
            pipeline,
            provider,
            appContextBuilder,
            "non-existing-environment",
            _explicitEnvironmentFilePath);

        Equal(1, result);

        var exists = environmentsRegistry.TryGetEnvironment("non-existing-environment", out var found);

        False(exists);
        Null(found);
    }

    [Fact]
    public async Task RegisterAllAvailableEnvironments()
    {
        PrepareServices(out var pipeline, out var provider, out _, out var environmentsRegistry, out var appContextBuilder);
        await RunApplicationPipeline(pipeline, provider, appContextBuilder);

        CheckExistenceOfEnvironment(Constants.DefaultEnvironmentName, environmentsRegistry);
        CheckExistenceOfEnvironment("test-lab", environmentsRegistry);
        CheckExistenceOfEnvironment("empty", environmentsRegistry);
        CheckExistenceOfEnvironment("allKind", environmentsRegistry);
    }

    [Fact]
    public async Task SetDefaultEnvironmentIfNoEnvironmentIsGivenAndImplicitEnvironmentFileIsUsed()
    {
        PrepareServices(
            out var pipeline, out var provider, out var variables, out var environmentsRegistry, out var appContextBuilder);

        await RunApplicationPipeline(pipeline, provider, appContextBuilder);

        CheckExistenceOfEnvironment(Constants.DefaultEnvironmentName, environmentsRegistry);
        CheckDefaultEnvironmentVariables(variables, true);
    }

    [Fact]
    public async Task SetDefaultEnvironmentIfNoEnvironmentIsGivenAndExplicitEnvironmentFileIsUsed()
    {
        PrepareServices(
            out var pipeline, out var provider, out var variables, out var environmentsRegistry, out var appContextBuilder);

        await RunApplicationPipeline(
            pipeline,
            provider,
            appContextBuilder,
            string.Empty,
            _explicitEnvironmentFilePath);

        CheckExistenceOfEnvironment(Constants.DefaultEnvironmentName, environmentsRegistry);
        CheckDefaultEnvironmentVariables(variables, true);
    }

    [Fact]
    public async Task SetGivenEnvironmentIfUsingImplicitEnvironmentFile()
    {
        PrepareServices(
            out var pipeline, out var provider, out var variables, out var environmentsRegistry, out var appContextBuilder);

        await RunApplicationPipeline(pipeline, provider, appContextBuilder, "test-lab");

        CheckTestlab(variables, environmentsRegistry);
    }

    [Fact]
    public async Task SetGivenEnvironmentIfUsingExplicitEnvironmentFile()
    {
        PrepareServices(
            out var pipeline, out var provider, out var variables, out var environmentsRegistry, out var appContextBuilder);

        await RunApplicationPipeline(pipeline, provider, appContextBuilder, "test-lab", _explicitEnvironmentFilePath);

        CheckTestlab(variables, environmentsRegistry);
    }

    [Fact]
    public async Task SetGivenEnvironmentIfImplicitEnvironmentFileIsNotOnTheZeroLevel()
    {
        PrepareServices(
            out var pipeline,
            out var provider,
            out var variables,
            out var environmentsRegistry,
            out var appContextBuilder,
            _collectionWithNestedEnvironmentFilePath);

        await RunApplicationPipeline(pipeline, provider, appContextBuilder, "test-lab");

        CheckTestlab(variables, environmentsRegistry);
    }

    [Fact]
    public async Task LoadDefaultEnvironmentVariablesEvenIfAnotherEnvironmentIsGiven()
    {
        PrepareServices(out var pipeline, out var provider, out var variables, out var environmentsRegistry, out var appContextBuilder);
        await RunApplicationPipeline(pipeline, provider, appContextBuilder, "test-lab");

        CheckExistenceOfEnvironment("test-lab", environmentsRegistry);
        CheckExistenceOfEnvironment(Constants.DefaultEnvironmentName, environmentsRegistry);
        CheckDefaultEnvironmentVariables(variables, false);
    }

    [Fact]
    public async Task OverrideVariablesFromDefaultEnvironmentWithSameNameVariablesFromGivenEnvironment()
    {
        PrepareServices(out var pipeline, out var provider, out var variables, out var environmentsRegistry, out var appContextBuilder);
        await RunApplicationPipeline(pipeline, provider, appContextBuilder, "test-lab");

        CheckExistenceOfEnvironment("test-lab", environmentsRegistry);
        CheckExistenceOfEnvironment(Constants.DefaultEnvironmentName, environmentsRegistry);
        Equal("http://localhost:3001", variables.GetVariable<string>("ApiBaseUrl"));
    }

    private static async Task<int> RunApplicationPipeline(
        IPipeline pipeline,
        IServiceProvider provider,
        ApplicationContextBuilder appContextBuilder,
        string environmentName = "",
        string environmentFilePath = "")
    {
        var structureExplorationStep = provider.GetStep<ExploreStructureStep>();
        var step = provider.GetStep<InitializeEnvironmentsStep>();

        pipeline.AddSteps(structureExplorationStep);
        pipeline.AddSteps(step);

        if (!environmentName.Equals(string.Empty))
        {
            appContextBuilder = appContextBuilder.WithEnvironment(environmentName);
        }

        if (!environmentFilePath.Equals(string.Empty))
        {
            appContextBuilder = appContextBuilder.WithEnvironmentFilePath(environmentFilePath);
        }

        var appContext = appContextBuilder
            .WithServiceProvider(provider)
            .WithReporter(provider.GetRequiredService<ITestResultsSummaryReporter>())
            .Build();

        return await pipeline.Run(appContext, CancellationToken.None);
    }

    private static void PrepareServices(
        out IPipeline pipeline,
        out IServiceProvider provider,
        out IVariables variables,
        out IEnvironmentsRegistry environmentsRegistry,
        out ApplicationContextBuilder appContextBuilder,
        string collectionPath = "")
    {
        var services = new ServiceCollection();
        services.AddScoped<ExploreStructureStep>();
        services.AddScoped<InitializeEnvironmentsStep>();
        services.AddScoped<SetEnvironmentStep>();
        services.AddScoped<ReportTestResultsSummaryStep>();
        services.AddSingleton<IVariables, global::TeaPie.Variables.Variables>();
        services.AddSingleton<IEnvironmentsRegistry, EnvironmentsRegistry>();
        services.AddSingleton<IPipeline, ApplicationPipeline>();
        services.AddSingleton<IStructureExplorer, StructureExplorer>();
        services.AddSingleton<ITestResultsSummaryReporter, TestResultsSummaryReporter>();
        services.AddLogging();

        provider = services.BuildServiceProvider();

        pipeline = provider.GetRequiredService<IPipeline>();
        variables = provider.GetRequiredService<IVariables>();
        environmentsRegistry = provider.GetRequiredService<IEnvironmentsRegistry>();

        appContextBuilder = new ApplicationContextBuilder()
            .WithPath(string.IsNullOrEmpty(collectionPath) ? _collectionPath : collectionPath)
            .WithServiceProvider(provider);
    }

    private static void CheckExistenceOfEnvironment(string environmentName, IEnvironmentsRegistry environmentsRegistry)
    {
        var hasEnv = environmentsRegistry.TryGetEnvironment(environmentName, out var foundEnv);

        True(hasEnv);
        NotNull(foundEnv);
    }

    private static void CheckDefaultEnvironmentVariables(IVariables variables, bool setAsEnvironment)
    {
        Equal("/customers", variables.GlobalVariables.Get<string>("ApiCustomersSection"));
        Equal("/cars", variables.GlobalVariables.Get<string>("ApiCarsSection"));
        Equal("/rental", variables.GlobalVariables.Get<string>("ApiCarRentalSection"));
        Equal("/customers", variables.GetVariable<string>("ApiCustomersSection"));
        Equal("/cars", variables.GetVariable<string>("ApiCarsSection"));
        Equal("/rental", variables.GetVariable<string>("ApiCarRentalSection"));

        if (setAsEnvironment)
        {
            Equal("/customers", variables.EnvironmentVariables.Get<string>("ApiCustomersSection"));
            Equal("/cars", variables.EnvironmentVariables.Get<string>("ApiCarsSection"));
            Equal("/rental", variables.EnvironmentVariables.Get<string>("ApiCarRentalSection"));
        }
    }

    private static void CheckTestlab(IVariables variables, IEnvironmentsRegistry environmentsRegistry)
    {
        CheckExistenceOfEnvironment("test-lab", environmentsRegistry);
        Equal("http://localhost:3001", variables.EnvironmentVariables.Get<string>("ApiBaseUrl"));
        Equal("stringValue", variables.EnvironmentVariables.Get<string>("StringVar"));
        True(variables.EnvironmentVariables.Get<bool>("BooleanVar"));
        Equal(25.6m, variables.EnvironmentVariables.Get<decimal>("DoubleVar"));
        Equal(199, variables.EnvironmentVariables.Get<int>("IntVar"));
        Equal(["one", "two", "three"], variables.EnvironmentVariables.Get<List<object>>("ListVar"));
    }
}
