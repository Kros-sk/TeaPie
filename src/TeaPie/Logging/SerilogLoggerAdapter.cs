using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace TeaPie.Logging;

public class SerilogLoggerAdapter(Serilog.ILogger logger) : ILogger
{
    private const string OriginalMessageKey = "{OriginalFormat}";

    private readonly Serilog.ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        // Serilog does not natively support BeginScope, but you can return a no-op disposable.
        return new NoopDisposable();
    }

    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(ConvertLogLevel(logLevel));

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (!TryGetOriginalMessageAndProperties(state, out var logMessage, out var propertyValues))
        {
            logMessage = formatter(state, exception);
        }

        _logger.Write(ConvertLogLevel(logLevel), exception, logMessage, propertyValues);
    }

    private static bool TryGetOriginalMessageAndProperties<TState>(
        TState state,
        out string originalMessage,
        out object?[]? properties)
    {
        originalMessage = string.Empty;
        properties = null;

        if (state is IReadOnlyList<KeyValuePair<string, object?>> pairs)
        {
            List<object?> propertyList = [];
            foreach (var pair in pairs)
            {
                if (pair.Key.Contains(OriginalMessageKey))
                {
                    if (pair.Value is string message)
                    {
                        originalMessage = message;
                    }
                }
                else
                {
                    propertyList.Add(pair.Value);
                }
            }

            if (!string.IsNullOrEmpty(originalMessage))
            {
                properties = [.. propertyList];
                return true;
            }
        }

        return false;
    }

    private static LogEventLevel ConvertLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            _ => LogEventLevel.Information,
        };
    }

    private class NoopDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
