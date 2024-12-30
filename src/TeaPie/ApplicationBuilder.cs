using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Http;
using TeaPie.Logging;
using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
using TeaPie.Testing;
using TeaPie.Variables;

namespace TeaPie;

public sealed class ApplicationBuilder
{
    private readonly IServiceCollection _services;
    private string _path;
    private string? _tempPath;

    private ApplicationBuilder(IServiceCollection services)
    {
        _services = services;
        _path = string.Empty;
    }

    public static ApplicationBuilder Create() => new(new ServiceCollection());

    public ApplicationBuilder WithPath(string path)
    {
        _path = path;
        return this;
    }

    public ApplicationBuilder WithTemporaryPath(string temporaryPath)
    {
        _tempPath = temporaryPath;
        return this;
    }

    public ApplicationBuilder AddLogging(
        LogLevel minimumLevel = LogLevel.Information,
        string pathToLogFile = "",
        LogLevel minimumLevelForLogFile = LogLevel.Information)
    {
        _services.ConfigureLogging(minimumLevel, pathToLogFile, minimumLevelForLogFile);
        return this;
    }

    public Application Build()
    {
        ConfigureServices();
        var provider = _services.BuildServiceProvider();

        CreateUserContext(provider);

        var applicationContext = GetApplicationContext(provider);

        var pipeline = BuildDefaultPipeline(provider);

        return new Application(pipeline, applicationContext);
    }

    private ApplicationContext GetApplicationContext(IServiceProvider provider)
        => new(
            _path,
            provider,
            provider.GetRequiredService<ICurrentTestCaseExecutionContextAccessor>(),
            provider.GetRequiredService<ILogger<ApplicationContext>>(),
            _tempPath ?? string.Empty);

    private void ConfigureServices()
    {
        _services.AddStructureExploration();
        _services.AddTestCases();
        _services.AddScripts();
        _services.AddHttp();
        _services.AddVariables();
        _services.AddTesting();
        _services.AddReporting();
        _services.AddPipelines();
    }

    private static TeaPie CreateUserContext(IServiceProvider provider)
        => TeaPie.Create(
            provider.GetRequiredService<IVariables>(),
            provider.GetRequiredService<ILogger<TeaPie>>(),
            provider.GetRequiredService<ITester>(),
            provider.GetRequiredService<ICurrentTestCaseExecutionContextAccessor>());

    private static ApplicationPipeline BuildDefaultPipeline(IServiceProvider provider)
    {
        var pipeline = provider.GetRequiredService<IPipeline>();
        pipeline.AddSteps(ApplicationStepsFactory.CreateDefaultPipelineSteps(provider));

        return (ApplicationPipeline)pipeline;
    }
}
