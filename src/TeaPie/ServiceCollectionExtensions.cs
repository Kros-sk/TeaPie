using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TeaPie.Logging;
using TeaPie.ScriptHandling;
using TeaPie.StructureExploration;

namespace TeaPie;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddSingleton<ILogger>(CreateSerilogLogger());
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger, SerilogLoggerAdapter>();

        services.AddSingleton<IStructureExplorer, StructureExplorer>();

        services.AddSingleton<IScriptPreProcessor, ScriptPreProcessor>();
        services.AddSingleton<IScriptCompiler, ScriptCompiler>();
        services.AddSingleton<INugetPackageHandler, NugetPackageHandler>();

        return services;
    }

    private static Serilog.Core.Logger CreateSerilogLogger()
        => new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
}
