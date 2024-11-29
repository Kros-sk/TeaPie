using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace TeaPie.Logging;

internal static class Setup
{
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
}
