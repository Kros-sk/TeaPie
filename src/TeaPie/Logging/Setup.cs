using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;

namespace TeaPie.Logging;

internal static class Setup
{
    public static IServiceCollection AddLogging(this IServiceCollection services, Action configure)
    {
        configure();

        services.AddTransient<LoggingInterceptorHandler>();
        services.AddTransient<RequestsLoggingHandler>();
        services.AddSingleton<NuGet.Common.ILogger, NuGetLoggerAdapter>();
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

        return services;
    }

    public static IServiceCollection ConfigureLogging(
        this IServiceCollection services,
        LogLevel minimumLevel,
        string pathToLogFile = "",
        LogLevel minimumLevelForLogFile = LogLevel.Debug,
        string? pathToRequestsLogFile = null,
        bool useTreeLogging = false)
    {
        TreeLoggingExtensions.SetTreeLoggingEnabled(useTreeLogging);
        if (minimumLevel == LogLevel.None)
        {
            Log.Logger = Serilog.Core.Logger.None;
        }
        else
        {
            var config = new LoggerConfiguration()
                .MinimumLevel.Is(GetMaximumFromMinimalLevels(minimumLevel, minimumLevelForLogFile))
                .MinimumLevel.Override("System.Net.Http", ApplyRestrictiveLogLevelRule(minimumLevel))
                .MinimumLevel.Override("TeaPie.Logging.NuGetLoggerAdapter", ApplyRestrictiveLogLevelRule(minimumLevel))
                .Enrich.FromLogContext();

            if (useTreeLogging)
            {
                AddTreeConsoleSink(config, minimumLevel);
            }
            else
            {
                AddConsoleSink(config, minimumLevel);
            }

            if (!pathToLogFile.Equals(string.Empty) && minimumLevelForLogFile < LogLevel.None)
            {
                AddLogFileSink(config, pathToLogFile, minimumLevelForLogFile);
            }

            if (!string.IsNullOrEmpty(pathToRequestsLogFile))
            {
                AddRequestsFileSink(config, pathToRequestsLogFile, minimumLevelForLogFile);
            }

            Log.Logger = config.CreateLogger();
        }

        return services;
    }

    private static LogEventLevel GetMaximumFromMinimalLevels(LogLevel minimumLevel1, LogLevel minimumLevel2)
        => minimumLevel1 <= minimumLevel2
            ? minimumLevel1.ToSerilogLogLevel()
            : minimumLevel2.ToSerilogLogLevel();

    private static LogEventLevel ApplyRestrictiveLogLevelRule(LogLevel minimumLevel)
        => minimumLevel >= LogLevel.Information ? LogEventLevel.Warning : LogEventLevel.Debug;

    private static void AddConsoleSink(LoggerConfiguration config, LogLevel minimumLevel)
    {
        config.WriteTo.Logger(lc => lc
            .Filter.ByExcluding(Matching.FromSource("HttpRequests"))
            .WriteTo.Console(restrictedToMinimumLevel: minimumLevel.ToSerilogLogLevel()));
    }

    private static void AddTreeConsoleSink(LoggerConfiguration config, LogLevel minimumLevel)
    {
        config
            .WriteTo.Logger(lc => lc
                .Filter.ByExcluding(Matching.FromSource("HttpRequests"))
                .WriteTo.TreeConsole(restrictedToMinimumLevel: minimumLevel.ToSerilogLogLevel()));
    }

    private static void AddLogFileSink(LoggerConfiguration config, string pathToLogFile, LogLevel minimumLevelForLogFile)
    {
        config.WriteTo.Logger(lc => lc
            .Filter.ByExcluding(Matching.FromSource("HttpRequests"))
            .WriteTo.File(pathToLogFile, restrictedToMinimumLevel: minimumLevelForLogFile.ToSerilogLogLevel()));
    }

    private static void AddRequestsFileSink(LoggerConfiguration config, string pathToRequestsLogFile, LogLevel minimumLevelForLogFile)
    {
        if (File.Exists(pathToRequestsLogFile))
        {
            File.Delete(pathToRequestsLogFile);
        }

        config.WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(Matching.FromSource("HttpRequests"))
            .WriteTo.File(
                new JsonFormatter(renderMessage: false),
                pathToRequestsLogFile,
                restrictedToMinimumLevel: minimumLevelForLogFile.ToSerilogLogLevel()));
    }
}
