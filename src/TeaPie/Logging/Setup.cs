using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace TeaPie.Logging;

internal static class Setup
{
    public static IServiceCollection AddLogging(this IServiceCollection services, Action configure)
    {
        configure();

        services.AddTransient<LoggingInterceptorHandler>();
        services.AddSingleton<NuGet.Common.ILogger, NuGetLoggerAdapter>();
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

        return services;
    }

    public static IServiceCollection ConfigureLogging(
        this IServiceCollection services,
        LogLevel minimumLevel,
        string pathToLogFile = "",
        LogLevel minimumLevelForLogFile = LogLevel.Debug,
        string structuredRequestsDirectory = "")
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
                .MinimumLevel.Override("TeaPie.Logging.NuGetLoggerAdapter", ApplyRestrictiveLogLevelRule(minimumLevel))
                .WriteTo.Console(restrictedToMinimumLevel: minimumLevel.ToSerilogLogLevel());

            if (!pathToLogFile.Equals(string.Empty) && minimumLevelForLogFile < LogLevel.None)
            {
                config.WriteTo.File(pathToLogFile, restrictedToMinimumLevel: minimumLevelForLogFile.ToSerilogLogLevel());
            }

            Log.Logger = config.CreateLogger();
        }

        if(!string.IsNullOrEmpty(structuredRequestsDirectory))
        {
            var logger = new StructuredRequestLogger(structuredRequestsDirectory);
            services.AddSingleton<IStructuredRequestLogger>(logger);
        }

        return services;
    }

    private static LogEventLevel GetMaximumFromMinimalLevels(LogLevel minimumLevel1, LogLevel minimumLevel2)
        => minimumLevel1 <= minimumLevel2
            ? minimumLevel1.ToSerilogLogLevel()
            : minimumLevel2.ToSerilogLogLevel();

    private static LogEventLevel ApplyRestrictiveLogLevelRule(LogLevel minimumLevel)
        => minimumLevel >= LogLevel.Information ? LogEventLevel.Warning : LogEventLevel.Debug;
}
