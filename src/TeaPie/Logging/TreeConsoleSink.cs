using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System.Text;

namespace TeaPie.Logging;

public class TreeConsoleSink(ITextFormatter formatter) : ILogEventSink
{
    private const string VerticalBar = "│  ";
    private const string StartCorner = "┌──";
    private const string EndCorner = "└──";

    private readonly ITextFormatter _formatter = formatter;

    private readonly object _pendingLock = new();
    private readonly SortedDictionary<int, LogEvent> _pendingStarts = new();

    public void Emit(LogEvent logEvent)
    {
        var depth = GetScopeDepth(logEvent);
        var message = logEvent.RenderMessage();

        if (message.StartsWith(StartCorner, StringComparison.Ordinal))
        {
            lock (_pendingLock)
            {
                _pendingStarts[depth] = logEvent;
            }
            return;
        }

        if (message.StartsWith(EndCorner, StringComparison.Ordinal))
        {
            lock (_pendingLock)
            {
                if (_pendingStarts.Remove(depth))
                {
                    return;
                }
            }

        }

        if (!message.StartsWith(StartCorner, StringComparison.Ordinal) && !message.StartsWith(EndCorner, StringComparison.Ordinal))
        {
            List<LogEvent>? toFlush = null;
            lock (_pendingLock)
            {
                if (_pendingStarts.Count > 0)
                {
                    toFlush = _pendingStarts.Keys.Where(d => d <= depth).OrderBy(d => d).Select(d => _pendingStarts[d]).ToList();
                    foreach (var d in toFlush.Select(e => GetScopeDepth(e!)))
                    {
                        _pendingStarts.Remove(d);
                    }
                }
            }

            if (toFlush != null)
            {
                foreach (var pending in toFlush)
                {
                    var pendingDepth = GetScopeDepth(pending);
                    var prefix = BuildPrefix(pendingDepth, pending);
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        var modifiedPending = AddPrefixToMessage(pending, prefix);
                        _formatter.Format(modifiedPending, Console.Out);
                    }
                    else
                    {
                        _formatter.Format(pending, Console.Out);
                    }
                }
            }
        }

        var finalPrefix = BuildPrefix(depth, logEvent);

        if (!string.IsNullOrEmpty(finalPrefix))
        {
            var modifiedEvent = AddPrefixToMessage(logEvent, finalPrefix);
            _formatter.Format(modifiedEvent, Console.Out);
        }
        else
        {
            _formatter.Format(logEvent, Console.Out);
        }
    }

    private static int GetScopeDepth(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue(ScopeDepthEnricher.ScopeDepthPropertyName, out var depthValue) && depthValue is ScalarValue { Value: int depth })
        {
            return depth;
        }

        return 0;
    }

    private static string BuildPrefix(int depth, LogEvent logEvent)
    {
        if (depth == 0)
        {
            return string.Empty;
        }

        var message = logEvent.RenderMessage();
        var isCornerMessage = message.StartsWith(StartCorner) || message.StartsWith(EndCorner);
        var visualDepth = isCornerMessage ? Math.Max(0, depth - 1) : depth;

        var builder = new StringBuilder();
        for (var i = 0; i < visualDepth; i++)
        {
            builder.Append(VerticalBar);
        }

        return builder.ToString();
    }

    private static LogEvent AddPrefixToMessage(LogEvent original, string prefix)
    {
        var newMessageTemplate = new Serilog.Parsing.MessageTemplateParser()
            .Parse(prefix + original.MessageTemplate.Text);

        return new LogEvent(
            original.Timestamp,
            original.Level,
            original.Exception,
            newMessageTemplate,
            original.Properties.Select(kvp => new LogEventProperty(kvp.Key, kvp.Value)));
    }
}

public static class TreeConsoleSinkExtensions
{
    public static LoggerConfiguration TreeConsole(
        this LoggerSinkConfiguration sinkConfiguration,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    {
        var formatter = new MessageTemplateTextFormatter(outputTemplate);
        return sinkConfiguration.Sink(new TreeConsoleSink(formatter), restrictedToMinimumLevel);
    }
}
