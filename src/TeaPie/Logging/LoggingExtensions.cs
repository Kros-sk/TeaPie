using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace TeaPie.Logging;

internal static class LoggingExtensions
{
    public static LogEventLevel ToSerilogLogLevel(this LogLevel level)
        => level switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            LogLevel.None => throw new ArgumentException("LogLevel.None cannot be converted to a Serilog level."),
            _ => throw new ArgumentOutOfRangeException(nameof(level), $"Unsupported LogLevel: {level}"),
        };

    public static LogLevel ToMicrosoftLogLevel(this NuGet.Common.LogLevel level)
        => level switch
        {
            NuGet.Common.LogLevel.Verbose => LogLevel.Trace,
            NuGet.Common.LogLevel.Debug => LogLevel.Debug,
            NuGet.Common.LogLevel.Information => LogLevel.Information,
            NuGet.Common.LogLevel.Minimal => LogLevel.Information,
            NuGet.Common.LogLevel.Warning => LogLevel.Warning,
            NuGet.Common.LogLevel.Error => LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(level), $"Unsupported NuGet.Common.LogLevel: {level}"),
        };
}
