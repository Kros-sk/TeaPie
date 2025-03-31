using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Reporting;
using TeaPie.StructureExploration.Paths;
using TeaPie.TestCases;
using TeaPie.Variables;
using static Xunit.Assert;

namespace TeaPie.Tests.Variables;

[Collection(nameof(NonParallelCollection))]
public class VariablesCachingShould
{
    private readonly string _variablesFilePath = Path.Combine(Environment.CurrentDirectory, "Demo", "Variables", "variables.json");

    [Fact]
    public async Task SkipLoadingOfVariablesIfThereIsNoCacheFile()
    {
        var serviceProvider = GetServiceProvider();

        var loadStep = serviceProvider.GetStep<TryLoadVariablesStep>();
        var applicationContext = GetApplicationContext(serviceProvider);

        await loadStep.Execute(applicationContext);

        var variables = serviceProvider.GetRequiredService<IVariables>();

        Empty(variables.GlobalVariables);
        Empty(variables.EnvironmentVariables);
        Empty(variables.CollectionVariables);
        Empty(variables.TestCaseVariables);
    }

    [Fact]
    public async Task SaveVariablesToFileCorrectly()
    {
        const string json = "{\"GlobalVariables\":{\"BaseUrl\":\"https://my-website-url.com/\"},\"EnvironmentVariables\":{\"BaseUrl\":\"https://localhost:8080/\",\"IsTestlab\":true},\"CollectionVariables\":{\"UserId\":1,\"CarId\":\"5f794223-4f3f-4d81-a68f-12a65a633c60\"},\"TestCaseVariables\":{\"JsonBody\":\"{ \\u0022id\\u0022:\\u002212\\u0022, \\u0022brand\\u0022:\\u0022Toyota\\u0022 }\"}}";
        var serviceProvider = GetServiceProvider();

        var saveStep = serviceProvider.GetStep<SaveVariablesStep>();
        var applicationContext = GetApplicationContext(serviceProvider);

        var variables = serviceProvider.GetRequiredService<IVariables>();

        AddVariables(variables);

        await saveStep.Execute(applicationContext);

        var content = await File.ReadAllTextAsync(_variablesFilePath);

        try
        {
            Equal(json, content);
        }
        finally
        {
            File.Delete(_variablesFilePath);
        }
    }

    [Fact]
    public async Task SaveAndThenLoadVariablesToFileCorrectly()
    {
        var serviceProvider = GetServiceProvider();

        var saveStep = serviceProvider.GetStep<SaveVariablesStep>();
        var loadStep = serviceProvider.GetStep<TryLoadVariablesStep>();
        var applicationContext = GetApplicationContext(serviceProvider);

        var variables = serviceProvider.GetRequiredService<IVariables>();

        AddVariables(variables);

        await saveStep.Execute(applicationContext);

        variables.GlobalVariables.Clear();
        variables.EnvironmentVariables.Clear();
        variables.CollectionVariables.Clear();
        variables.TestCaseVariables.Clear();

        await loadStep.Execute(applicationContext);

        try
        {
            Equal("https://my-website-url.com/", variables.GlobalVariables.Get<string>("BaseUrl"));
            Equal("https://localhost:8080/", variables.EnvironmentVariables.Get<string>("BaseUrl"));
            True(variables.EnvironmentVariables.Get<bool>("IsTestlab"));
            Equal(1, variables.CollectionVariables.Get<int>("UserId"));
            Equal(Guid.Parse("5f794223-4f3f-4d81-a68f-12a65a633c60"), variables.CollectionVariables.Get<Guid>("CarId"));
            Equal("{ \"id\":\"12\", \"brand\":\"Toyota\" }", variables.TestCaseVariables.Get<string>("JsonBody"));
        }
        finally
        {
            File.Delete(_variablesFilePath);
        }
    }

    private static void AddVariables(IVariables variables)
    {
        variables.GlobalVariables.Set("BaseUrl", "https://my-website-url.com/");
        variables.EnvironmentVariables.Set("BaseUrl", "https://localhost:8080/");
        variables.EnvironmentVariables.Set("IsTestlab", true);
        variables.CollectionVariables.Set("UserId", 1);
        variables.CollectionVariables.Set("CarId", "5f794223-4f3f-4d81-a68f-12a65a633c60");
        variables.TestCaseVariables.Set("JsonBody", "{ \"id\":\"12\", \"brand\":\"Toyota\" }");
    }

    private static ApplicationContext GetApplicationContext(ServiceProvider serviceProvider)
        => new(
            string.Empty,
            serviceProvider,
            serviceProvider.GetRequiredService<ICurrentTestCaseExecutionContextAccessor>(),
            serviceProvider.GetRequiredService<ITestResultsSummaryReporter>(),
            serviceProvider.GetRequiredService<ILogger<ApplicationContext>>(),
            new ApplicationContextOptions(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));

    private ServiceProvider GetServiceProvider()
    {
        var pathProvider = Substitute.For<IPathProvider>();
        pathProvider.VariablesFilePath.Returns(_variablesFilePath);

        var services = new ServiceCollection();
        services.AddTeaPie(true, () => { });

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPathProvider));
        services.Remove(descriptor!);
        services.AddSingleton(pathProvider);

        return services.BuildServiceProvider();
    }
}
