using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.TestCases;

namespace TeaPie.Tests;

internal class ApplicationContextBuilder
{
    private static string? _path;
    private static string? _tempFolderPath;
    private static string? _environmentName;
    private static string? _environmentFilePath;
    private static ILogger? _logger;
    private static IServiceProvider? _serviceProvider;
    private static ICurrentTestCaseExecutionContextAccessor? _currentTestCaseExecutionContextAccessor;

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

    public ApplicationContextBuilder WithLogger(ILogger logger)
    {
        _logger = logger;
        return this;
    }

    public ApplicationContextBuilder WithTempFolderPath(string tempFolderPath)
    {
        _tempFolderPath = tempFolderPath;
        return this;
    }

    public ApplicationContextBuilder WithCurrentTestCaseExecutionContextAccessor(
        ICurrentTestCaseExecutionContextAccessor currentTestCaseExecutionContextAccessor)
    {
        _currentTestCaseExecutionContextAccessor = currentTestCaseExecutionContextAccessor;
        return this;
    }

    public ApplicationContextBuilder WithEnvironment(string environmentName)
    {
        _environmentName = environmentName;
        return this;
    }

    public ApplicationContextBuilder WithEnvironmentFilePath(string environmentFilePath)
    {
        _environmentFilePath = environmentFilePath;
        return this;
    }

    public ApplicationContext Build()
        => new(
            _path ?? string.Empty,
            _serviceProvider ?? Substitute.For<IServiceProvider>(),
            _currentTestCaseExecutionContextAccessor ?? Substitute.For<ICurrentTestCaseExecutionContextAccessor>(),
            _logger ?? Substitute.For<ILogger>(),
            _tempFolderPath ?? string.Empty,
            _environmentName ?? string.Empty,
            _environmentFilePath ?? string.Empty);
}
