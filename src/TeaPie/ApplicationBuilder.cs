using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Http;
using TeaPie.Logging;
using TeaPie.Pipelines;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
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

    public ApplicationBuilder AddLogging(LogLevel minimumLevel = LogLevel.Information, string pathToLogFile = "")
    {
        _services.ConfigureLogging(minimumLevel, pathToLogFile);
        return this;
    }

    public Application Build()
    {
        ConfigureServices();
        var provider = _services.BuildServiceProvider();

        CreateUserContext(provider);

        var applicationContext =
            new ApplicationContext(
                _path,
                provider.GetRequiredService<ILogger<ApplicationContext>>(),
                provider,
                _tempPath ?? string.Empty);

        var pipeline = BuildDefaultPipeline(provider);

        return new Application(pipeline, applicationContext);
    }

    private void ConfigureServices()
    {
        _services.AddStructureExploration();
        _services.AddTestCases();
        _services.AddScripts();
        _services.AddHttp();
        _services.AddVariables();
        _services.AddPipelines();
    }

    private static TeaPie CreateUserContext(IServiceProvider provider)
        => TeaPie.Create(provider.GetRequiredService<IVariables>(), provider.GetRequiredService<ILogger<TeaPie>>());

    // TODO: This should be part of some pipeline builder/factory class
    private static ApplicationPipeline BuildDefaultPipeline(IServiceProvider provider)
    {
        var pipeline = provider.GetRequiredService<IPipeline>();
        pipeline.AddSteps(provider.GetStep<StructureExplorationStep>());
        pipeline.AddSteps(provider.GetStep<PrepareTemporaryFolderStep>());
        pipeline.AddSteps(provider.GetStep<GenerateStepsForTestCasesStep>());

        return (ApplicationPipeline)pipeline;
    }
}
