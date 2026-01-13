using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System.Text;

namespace TeaPie.Logging;

public class TreeConsoleSink : ILogEventSink
{
    private const string VerticalBar = "│  ";
    private const string StartCorner = "┌──";
    private const string EndCorner = "└──";

    private readonly ITextFormatter _formatter;

    public TreeConsoleSink(ITextFormatter formatter)
    {
        _formatter = formatter;
    }

    public void Emit(LogEvent logEvent)
    {
        var depth = GetScopeDepth(logEvent);
        var prefix = BuildPrefix(depth, logEvent);

        if (!string.IsNullOrEmpty(prefix))
        {
            var modifiedEvent = AddPrefixToMessage(logEvent, prefix);
            _formatter.Format(modifiedEvent, Console.Out);
        }
        else
        {
            _formatter.Format(logEvent, Console.Out);
        }
    }

    private static int GetScopeDepth(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue(ScopeDepthEnricher.ScopeDepthPropertyName, out var depthValue))
        {
            if (depthValue is ScalarValue { Value: int depth })
            {
                return depth;
            }
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
