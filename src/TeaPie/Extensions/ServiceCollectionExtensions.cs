using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;
using TeaPie.Pipelines;
using TeaPie.Pipelines.Scripts;
using TeaPie.ScriptHandling;
using TeaPie.StructureExploration;

namespace TeaPie.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddSingleton<IStructureExplorer, StructureExplorer>();

        services.AddSingleton<IScriptPreProcessor, ScriptPreProcessor>();
        services.AddSingleton<IScriptCompiler, ScriptCompiler>();
        services.AddSingleton<INugetPackageHandler, NugetPackageHandler>();

        return services;
    }

    public static IServiceCollection ConfigureLogging(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));

        return services;
    }

    public static IServiceCollection ConfigureSteps(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var implementation in FindImplementations<IPipelineStep>(assembly))
        {
            services.AddTransient(implementation);
        }

        return services;
    }

    public static IServiceCollection ConfigureAccessors(this IServiceCollection services)
    {
        services.AddScoped<IScriptExecutionContextAccessor, ScriptExecutionContextAccessor>();
        return services;
    }

    private static IEnumerable<Type> FindImplementations<TInterface>(Assembly assembly)
        => assembly.GetTypes()
            .Where(t => typeof(TInterface).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
}
