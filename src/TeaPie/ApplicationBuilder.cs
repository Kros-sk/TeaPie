using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Functions;
using TeaPie.Http.Auth;
using TeaPie.Http.Retrying;
using TeaPie.Logging;
using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.Scripts;
using TeaPie.TestCases;
using TeaPie.Testing;
using TeaPie.Variables;

namespace TeaPie;

public sealed class ApplicationBuilder
{
    private readonly IServiceCollection _services;

    private readonly bool _isCollectionRun;

    private string? _path;
    private string? _tempPath;
    private string? _scriptPath;

    private string? _environment;
    private string? _environmentFilePath;
    private string? _reportFilePath;
    private string? _initializationScriptPath;

    private LogLevel _minimumLogLevel = LogLevel.None;
    private string _pathToLogFile = string.Empty;
    private LogLevel _minimumLevelForLogFile = LogLevel.None;
    private string? _pathToRequestsLogFile;

    private bool _variablesCaching = true;

    private Func<IServiceProvider, IPipelineStep[]> _pipelineBuildFunction = ApplicationStepsFactory.CreateDefaultPipelineSteps;

    private ApplicationBuilder(IServiceCollection services, bool collectionRun)
    {
        _services = services;
        _isCollectionRun = collectionRun;
    }

    public static ApplicationBuilder Create(bool collectionRun = true) => new(new ServiceCollection(), collectionRun);

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
        LogLevel minimumLevelForLogFile = LogLevel.None,
        string? pathToRequestsLogFile = null)
    {
        _minimumLogLevel = minimumLevel;
        _pathToLogFile = pathToLogFile;
        _minimumLevelForLogFile = minimumLevelForLogFile;
        _pathToRequestsLogFile = pathToRequestsLogFile;
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

    public ApplicationBuilder WithScriptCompilationPipeline(string scriptPath)
    {
        _scriptPath = scriptPath;
        _pipelineBuildFunction = ApplicationStepsFactory.CreateScriptCompilationSteps;
        return this;
    }

    public ApplicationBuilder WithEnvironment(string environmentName)
    {
        _environment = environmentName;
        return this;
    }

    public ApplicationBuilder WithEnvironmentFile(string environmentFilePath)
    {
        _environmentFilePath = environmentFilePath;
        return this;
    }

    public ApplicationBuilder WithReportFile(string reportFilePath)
    {
        _reportFilePath = reportFilePath;
        return this;
    }

    public ApplicationBuilder WithInitializationScript(string initializationScriptPath)
    {
        _initializationScriptPath = initializationScriptPath;
        return this;
    }

    public ApplicationBuilder WithVariablesCaching(bool cacheVariables)
    {
        _variablesCaching = cacheVariables;
        return this;
    }

    public Application Build()
    {
        ConfigureServices();
        var provider = _services.BuildServiceProvider();

        var applicationContext = GetApplicationContext(provider);

        CreateUserContext(provider, applicationContext);

        var pipeline = BuildPipeline(provider);

        return new Application(pipeline, applicationContext);
    }

    private ApplicationContext GetApplicationContext(IServiceProvider provider)
    {
        var options = new ApplicationContextOptionsBuilder()
            .SetTempFolderPath(_tempPath)
            .SetEnvironment(_environment)
            .SetEnvironmentFilePath(_environmentFilePath)
            .SetReportFilePath(_reportFilePath)
            .SetInitializationScriptPath(_initializationScriptPath)
            .SetVariablesCaching(_variablesCaching)
            .Build();

        return new ApplicationContext(
            string.IsNullOrEmpty(_path) ? Directory.GetCurrentDirectory() : _path,
            provider,
            provider.GetRequiredService<ICurrentTestCaseExecutionContextAccessor>(),
            provider.GetRequiredService<ITestResultsSummaryReporter>(),
            provider.GetRequiredService<ILogger<ApplicationContext>>(),
            options);
    }

    private void ConfigureServices()
        => _services.AddTeaPie(
            _isCollectionRun,
            () => _services.ConfigureLogging(_minimumLogLevel, _pathToLogFile, _minimumLevelForLogFile, _pathToRequestsLogFile));

    private static TeaPie CreateUserContext(IServiceProvider provider, ApplicationContext applicationContext)
        => TeaPie.Create(
            applicationContext,
            provider,
            provider.GetRequiredService<IVariables>(),
            provider.GetRequiredService<IFunctions>(),
            provider.GetRequiredService<ILogger<TeaPie>>(),
            provider.GetRequiredService<IRegistrator>(),
            provider.GetRequiredService<ICurrentTestCaseExecutionContextAccessor>(),
            provider.GetRequiredService<ITestResultsSummaryReporter>(),
            provider.GetRequiredService<IRetryStrategyRegistry>(),
            provider.GetRequiredService<IAuthProviderRegistry>(),
            provider.GetRequiredService<IAuthProviderAccessor>(),
            provider.GetRequiredService<ITestFactory>(),
            provider.GetRequiredService<IHttpClientFactory>());

    private ApplicationPipeline BuildPipeline(IServiceProvider provider)
        => !string.IsNullOrEmpty(_scriptPath)
            ? GetScriptCompilationPipeline(provider)
            : GetPipeline(provider);

    private ApplicationPipeline GetScriptCompilationPipeline(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var accessor = provider.GetRequiredService<IScriptExecutionContextAccessor>();
        accessor.Context = new ScriptExecutionContext(
            new StructureExploration.Script(new StructureExploration.File(_scriptPath!)));

        return GetPipeline(provider);
    }

    private ApplicationPipeline GetPipeline(IServiceProvider provider)
    {
        var pipeline = provider.GetRequiredService<IPipeline>();
        pipeline.AddSteps(_pipelineBuildFunction.Invoke(provider));
        return (ApplicationPipeline)pipeline;
    }
}
