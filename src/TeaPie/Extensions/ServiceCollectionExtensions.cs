using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Reflection;
using TeaPie.Logging;
using TeaPie.Parsing;
using TeaPie.Pipelines;
using TeaPie.Pipelines.Requests;
using TeaPie.Pipelines.Scripts;
using TeaPie.Requests;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.Variables;

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

        services.AddSingleton<IVariables, Variables.Variables>();

        return services;
    }

    public static IServiceCollection ConfigureLogging(
        this IServiceCollection services,
        LogLevel minimumLevel,
        string pathToLogFile = "")
    {
        if (minimumLevel == LogLevel.None)
        {
            Log.Logger = Serilog.Core.Logger.None;
        }
        else
        {
            var config = new LoggerConfiguration()
                .MinimumLevel.Is(minimumLevel.ToSerilogLogLevel())
                .MinimumLevel.Override("System.Net.Http", ApplyRestrictiveLogLevelRule(minimumLevel))
                .MinimumLevel.Override("TeaPie.Logging.NuGetLoggerAdapter", ApplyRestrictiveLogLevelRule(minimumLevel))
                .WriteTo.Console();

            if (!pathToLogFile.Equals(string.Empty))
            {
                config.WriteTo.File(pathToLogFile);
            }

            Log.Logger = config.CreateLogger();
        }

        services.AddSingleton<NuGet.Common.ILogger, NuGetLoggerAdapter>();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

        return services;
    }

    private static LogEventLevel ApplyRestrictiveLogLevelRule(LogLevel minimumLevel)
        => minimumLevel >= LogLevel.Information ? LogEventLevel.Warning : LogEventLevel.Debug;

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
