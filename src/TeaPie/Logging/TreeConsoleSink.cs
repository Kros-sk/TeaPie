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

    private readonly ITextFormatter _formatter = formatter;

    public void Emit(LogEvent logEvent)
    {
        var stack = TreeScopeStateStore.GetStack();
        if (stack != null && stack.Count > 0)
        {
            foreach (var s in stack)
            {
                if (!s.Printed)
                {
                    TreeConsoleWriter.WriteOpening(s.Depth, logEvent.Timestamp, TreeConsoleWriter.LevelToShort(logEvent.Level));
                    TreeScopeStateStore.MarkPrinted(s);
                }
            }
        }

        var printedCount = stack?.Count(s => s.Printed) ?? 0;
        var prefixBuilder = new StringBuilder();
        for (var i = 0; i < printedCount; i++)
        {
            prefixBuilder.Append(VerticalBar);
        }
        var finalPrefix = prefixBuilder.ToString();

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
