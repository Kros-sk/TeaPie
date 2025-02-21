using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Reporting;
using TeaPie.TestCases;

namespace TeaPie.Tests;

internal class ApplicationContextBuilder
{
    private string _path = string.Empty;
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();
    private IServiceProvider? _serviceProvider;
    private ILogger<ApplicationContext>? _logger;
    private ICurrentTestCaseExecutionContextAccessor? _currentTestCaseExecutionContextAccessor;
    private ITestResultsSummaryReporter? _testResultsSummaryReporter;
    private readonly ApplicationContextOptionsBuilder _optionsBuilder = new();

    public ApplicationContextBuilder WithPath(string path)
    {
        _path = path;
        return this;
    }

    public ApplicationContextBuilder WithServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        return this;
    }

    public ApplicationContextBuilder WithLogger(ILogger<ApplicationContext> logger)
    {
        _logger = logger;
        return this;
    }

    public ApplicationContextBuilder WithCurrentTestCaseExecutionContextAccessor(
        ICurrentTestCaseExecutionContextAccessor accessor)
    {
        _currentTestCaseExecutionContextAccessor = accessor;
        return this;
    }

    public ApplicationContextBuilder WithReporter(ITestResultsSummaryReporter reporter)
    {
        _testResultsSummaryReporter = reporter;
        return this;
    }

    public ApplicationContextBuilder WithTempFolderPath(string tempFolderPath)
    {
        _optionsBuilder.SetTempFolderPath(tempFolderPath);
        return this;
    }

    public ApplicationContextBuilder WithEnvironment(string environmentName)
    {
        _optionsBuilder.SetEnvironment(environmentName);
        return this;
    }

    public ApplicationContextBuilder WithEnvironmentFilePath(string environmentFilePath)
    {
        _optionsBuilder.SetEnvironmentFilePath(environmentFilePath);
        return this;
    }

    public ApplicationContextBuilder WithReportFilePath(string reportFilePath)
    {
        _optionsBuilder.SetReportFilePath(reportFilePath);
        return this;
    }

    public ApplicationContextBuilder WithInitializationScriptPath(string initializationScriptPath)
    {
        _optionsBuilder.SetInitializationScriptPath(initializationScriptPath);
        return this;
    }

    public ApplicationContext Build()
    {
        _serviceCollection.TryAddSingleton(_ =>
            _currentTestCaseExecutionContextAccessor ?? Substitute.For<ICurrentTestCaseExecutionContextAccessor>());

        _serviceCollection.TryAddSingleton(_ =>
            _testResultsSummaryReporter ?? Substitute.For<ITestResultsSummaryReporter>());

        _serviceCollection.TryAddSingleton(_
            => _logger ?? Substitute.For<ILogger<ApplicationContext>>());

        var finalServiceProvider = _serviceProvider ?? _serviceCollection.BuildServiceProvider();

        return new ApplicationContext(
            _path,
            finalServiceProvider,
            _optionsBuilder.Build()
        );
    }
}
