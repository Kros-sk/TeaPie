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
using TeaPie.Scripts;
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
        services.AddSingleton<IHttpRequestHeadersProvider, HttpRequestHeadersProvider>();

        return services;
    }

    public static IServiceCollection ConfigureLogging(this IServiceCollection services, LogLevel minimumLevel)
    {
        if (minimumLevel == LogLevel.None)
        {
            Log.Logger = Serilog.Core.Logger.None;
        }
        else
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(minimumLevel.ToSerilogLevel())
                .MinimumLevel.Override(
                    "System.Net.Http",
                    minimumLevel >= LogLevel.Information ? LogEventLevel.Warning : LogEventLevel.Debug)
                .WriteTo.Console()
                .CreateLogger();
        }

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

        return services;
    }

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
        services.AddHttpClient<HttpRequestHeadersProvider>();
        services.AddHttpClient<ExecuteScriptStep>();

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
