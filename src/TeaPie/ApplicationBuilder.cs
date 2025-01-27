using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Environments;
using TeaPie.Http;
using TeaPie.Logging;
using TeaPie.Pipelines;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
using TeaPie.Testing;
using TeaPie.Variables;

namespace TeaPie;

public sealed class ApplicationBuilder
{
    private readonly IServiceCollection _services;

    private string? _path;
    private string? _tempPath;

    private LogLevel _minimumLogLevel = LogLevel.None;
    private string _pathToLogFile = string.Empty;
    private LogLevel _minimumLevelForLogFile = LogLevel.None;

    private Func<IServiceProvider, IPipelineStep[]> _pipelineBuildFunction = ApplicationStepsFactory.CreateDefaultPipelineSteps;

    private ApplicationBuilder(IServiceCollection services)
    {
        _services = services;
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

    public ApplicationBuilder WithLogging(
        LogLevel minimumLevel,
        string pathToLogFile = "",
        LogLevel minimumLevelForLogFile = LogLevel.None)
    {
        _minimumLogLevel = minimumLevel;
        _pathToLogFile = pathToLogFile;
        _minimumLevelForLogFile = minimumLevelForLogFile;
        return this;
    }

    public ApplicationBuilder WithDefaultPipeline()
    {
        _pipelineBuildFunction = ApplicationStepsFactory.CreateDefaultPipelineSteps;
        return this;
    }

    public ApplicationBuilder WithStructureExplorationPipeline()
    {
        _pipelineBuildFunction = ApplicationStepsFactory.CreateStructureExplorationSteps;
        return this;
    }

    public Application Build()
    {
        ConfigureServices();
        var provider = _services.BuildServiceProvider();

        var applicationContext = GetApplicationContext(provider);

        CreateUserContext(provider, applicationContext);

        var pipeline = BuildDefaultPipeline(provider);

        return new Application(pipeline, applicationContext);
    }

    private ApplicationContext GetApplicationContext(IServiceProvider provider)
        => new(
            string.IsNullOrEmpty(_path) ? Directory.GetCurrentDirectory() : _path,
            provider,
            provider.GetRequiredService<ICurrentTestCaseExecutionContextAccessor>(),
            provider.GetRequiredService<ILogger<ApplicationContext>>(),
            string.IsNullOrEmpty(_tempPath) ? Constants.DefaultTemporaryFolderPath : _tempPath);

    private void ConfigureServices()
    {
        _services.AddStructureExploration();
        _services.AddEnvironments();
        _services.AddTestCases();
        _services.AddScripts();
        _services.AddHttp();
        _services.AddVariables();
        _services.AddTesting();
        _services.AddPipelines();
        _services.AddLogging(() => _services.ConfigureLogging(_minimumLogLevel, _pathToLogFile, _minimumLevelForLogFile));
    }

    private static TeaPie CreateUserContext(IServiceProvider provider, ApplicationContext applicationContext)
        => TeaPie.Create(
            provider.GetRequiredService<IVariables>(),
            provider.GetRequiredService<ILogger<TeaPie>>(),
            provider.GetRequiredService<ITester>(),
            provider.GetRequiredService<ICurrentTestCaseExecutionContextAccessor>(),
            applicationContext,
            provider.GetRequiredService<IPipeline>());

    private ApplicationPipeline BuildDefaultPipeline(IServiceProvider provider)
    {
        var pipeline = provider.GetRequiredService<IPipeline>();
        pipeline.AddSteps(_pipelineBuildFunction.Invoke(provider));

        return (ApplicationPipeline)pipeline;
    }
}
