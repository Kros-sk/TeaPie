using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Variables;

namespace TeaPie.Tests;

internal class ApplicationContextBuilder
{
    private static string? _path;
    private static string? _tempFolderPath;
    private static ILogger? _logger;
    private static IServiceProvider? _serviceProvider;
    private static TeaPie? _userContext;

    public ApplicationContextBuilder WithPath(string path)
    {
        _path = path;
        return this;
    }

    public ApplicationContextBuilder WithTempFolderPath(string tempFolderPath)
    {
        _tempFolderPath = tempFolderPath;
        return this;
    }

    public ApplicationContextBuilder WithLogger(ILogger logger)
    {
        _logger = logger;
        return this;
    }

    public ApplicationContextBuilder WithServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        return this;
    }

    public ApplicationContextBuilder WithUserContext(TeaPie userContext)
    {
        _userContext = userContext;
        return this;
    }

    public ApplicationContext Build()
        => new(
            _path ?? string.Empty,
            _logger ?? Substitute.For<ILogger>(),
            _serviceProvider ?? Substitute.For<IServiceProvider>(),
            _userContext ?? TeaPie.Create(Substitute.For<IVariables>(), Substitute.For<ILogger>()),
            _tempFolderPath ?? string.Empty);
}
