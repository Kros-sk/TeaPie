using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Reflection;
using TeaPie.Parsing;
using TeaPie.Pipelines;
using TeaPie.Pipelines.Requests;
using TeaPie.Pipelines.Scripts;
using TeaPie.Requests;
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
        services.AddSingleton<INuGetPackageHandler, NuGetPackageHandler>();

        services.AddSingleton<IHttpFileParser, HttpFileParser>();
        services.AddSingleton<IRequestSender, RequestSender>();

        return services;
    }

    public static IServiceCollection ConfigureLogging(this IServiceCollection services, LogLevel minimumLevel)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(ConvertToSerilogLevel(minimumLevel))
            .MinimumLevel.Override(
                "System.Net.Http",
                minimumLevel >= LogLevel.Information ? LogEventLevel.Warning : LogEventLevel.Debug)
            .WriteTo.Console()
            .CreateLogger();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

        return services;
    }

    private static LogEventLevel ConvertToSerilogLevel(LogLevel minimumLevel)
        => minimumLevel switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            LogLevel.None => LogEventLevel.Fatal,
            _ => LogEventLevel.Information,
        };

    public static IServiceCollection AddSteps(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var implementation in FindImplementations<IPipelineStep>(assembly))
        {
            services.AddTransient(implementation);
        }

        return services;
    }

    public static IServiceCollection ConfigureHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient<IRequestSender, RequestSender>();

        return services;
    }

    public static IServiceCollection ConfigureAccessors(this IServiceCollection services)
    {
        services.AddScoped<IScriptExecutionContextAccessor, ScriptExecutionContextAccessor>();
        services.AddScoped<IRequestExecutionContextAccessor, RequestExecutionContextAccessor>();

        return services;
    }

    private static IEnumerable<Type> FindImplementations<TInterface>(Assembly assembly)
        => assembly.GetTypes()
            .Where(t => typeof(TInterface).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
}
