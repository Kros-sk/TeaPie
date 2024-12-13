using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Testing;
using TeaPie.Variables;

namespace TeaPie.Tests;

internal class ApplicationContextBuilder
{
    private static string? _path;
    private static string? _tempFolderPath;
    private static ILogger? _logger;
    private static IServiceProvider? _serviceProvider;
    private static TeaPie? _userContext;
    private static ITester? _tester;

    public ApplicationContextBuilder WithPath(string path)
    {
        _path = path;
        return this;
    }

    public ApplicationContextBuilder WithUserContext(TeaPie userContext)
    {
        _userContext = userContext;
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

    public ApplicationContextBuilder WithTester(ITester tester)
    {
        _tester = tester;
        return this;
    }

    public ApplicationContext Build()
        => new(
            _path ?? string.Empty,
            _userContext ?? TeaPie.Create(Substitute.For<IVariables>(), Substitute.For<ILogger>(), Substitute.For<ITester>()),
            _serviceProvider ?? Substitute.For<IServiceProvider>(),
            _tester ?? Substitute.For<ITester>(),
            _logger ?? Substitute.For<ILogger>(),
            _tempFolderPath ?? string.Empty);
}
