using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace TeaPie.Extensions;
internal static class LoggingExtensions
{
    public static LogEventLevel ToSerilogLevel(this LogLevel minimumLevel)
        => minimumLevel switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            LogLevel.None => throw new ArgumentException("LogLevel.None cannot be converted to a Serilog level."),
            _ => throw new ArgumentOutOfRangeException(nameof(minimumLevel), $"Unsupported LogLevel: {minimumLevel}"),
        };
}
