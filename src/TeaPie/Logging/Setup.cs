using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
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
        string? pathToRequestsLogFile = null)
    {
        if (minimumLevel == LogLevel.None)
        {
            Log.Logger = Serilog.Core.Logger.None;
        }
        else
        {
            var config = new LoggerConfiguration()
                .MinimumLevel.Is(GetMaximumFromMinimalLevels(minimumLevel, minimumLevelForLogFile))
                .MinimumLevel.Override("System.Net.Http", ApplyRestrictiveLogLevelRule(minimumLevel))
                .MinimumLevel.Override("TeaPie.Logging.NuGetLoggerAdapter", ApplyRestrictiveLogLevelRule(minimumLevel));

            var hasRequestsLogFile = !string.IsNullOrEmpty(pathToRequestsLogFile);

            AddConsoleSink(config, minimumLevel, hasRequestsLogFile);

            if (!pathToLogFile.Equals(string.Empty) && minimumLevelForLogFile < LogLevel.None)
            {
                AddLogFileSink(config, pathToLogFile, minimumLevelForLogFile, hasRequestsLogFile);
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

    private static void AddConsoleSink(LoggerConfiguration config, LogLevel minimumLevel, bool excludeRequests)
    {
        if (excludeRequests)
        {
            config.WriteTo.Logger(lc => lc
                .Filter.ByExcluding(IsHttpRequestLog)
                .WriteTo.Console(restrictedToMinimumLevel: minimumLevel.ToSerilogLogLevel()));
        }
        else
        {
            config.WriteTo.Console(restrictedToMinimumLevel: minimumLevel.ToSerilogLogLevel());
        }
    }

    private static void AddLogFileSink(LoggerConfiguration config, string pathToLogFile, LogLevel minimumLevelForLogFile, bool excludeRequests)
    {
        if (excludeRequests)
        {
            config.WriteTo.Logger(lc => lc
                .Filter.ByExcluding(IsHttpRequestLog)
                .WriteTo.File(pathToLogFile, restrictedToMinimumLevel: minimumLevelForLogFile.ToSerilogLogLevel()));
        }
        else
        {
            config.WriteTo.File(pathToLogFile, restrictedToMinimumLevel: minimumLevelForLogFile.ToSerilogLogLevel());
        }
    }

    private static void AddRequestsFileSink(LoggerConfiguration config, string pathToRequestsLogFile, LogLevel minimumLevelForLogFile)
    {
        if (File.Exists(pathToRequestsLogFile))
        {
            File.Delete(pathToRequestsLogFile);
        }

        config.WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(IsHttpRequestLog)
            .WriteTo.File(
                new JsonFormatter(renderMessage: false),
                pathToRequestsLogFile,
                restrictedToMinimumLevel: minimumLevelForLogFile.ToSerilogLogLevel()));
    }

    private static bool IsHttpRequestLog(Serilog.Events.LogEvent logEvent)
        => logEvent.Properties.TryGetValue("SourceContext", out var sourceContext) &&
           sourceContext.ToString().Contains("HttpRequests");
}
