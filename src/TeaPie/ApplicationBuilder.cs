using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Extensions;
using TeaPie.Pipelines;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.StructureExploration;
using TeaPie.Pipelines.TemporaryFolder;
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
        ConfigureDefaultServices();
        RegisterPipeline();
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

    private void ConfigureDefaultServices()
    {
        _services.ConfigureServices();
        _services.ConfigureAccessors();
        _services.ConfigureHttpClient();
        _services.AddSteps();
    }

    private void RegisterPipeline() => _services.AddSingleton<IPipeline, ApplicationPipeline>();

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
