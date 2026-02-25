using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace TeaPie.Logging;

internal class TreeConsoleSink(ITextFormatter formatter) : ILogEventSink
{
    private static readonly Serilog.Parsing.MessageTemplateParser _parser = new();
    private readonly ITextFormatter _formatter = formatter;

    public void Emit(LogEvent logEvent)
    {
        var scopes = TreeScopeStateStore.GetActiveScopes();
        var printedCount = PrintUnopenedScopes(scopes, logEvent);
        var prefix = TreeConsoleWriter.BuildPrefix(printedCount);
        WriteLogEvent(logEvent, prefix);
    }

    private static int PrintUnopenedScopes(IReadOnlyList<TreeScopeStateStore.ScopeState>? scopes, LogEvent logEvent)
    {
        if (scopes == null || scopes.Count == 0)
        {
            return 0;
        }

        var printedCount = 0;
        foreach (var s in scopes)
        {
            if (!s.Printed)
            {
                TreeConsoleWriter.WriteOpening(s.Depth, logEvent.Timestamp, TreeConsoleWriter.LevelToShort(logEvent.Level));
                TreeScopeStateStore.MarkPrinted(s, logEvent.Level);
            }

            printedCount++;
        }

        return printedCount;
    }

    private void WriteLogEvent(LogEvent logEvent, string prefix)
    {
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

    private static LogEvent AddPrefixToMessage(LogEvent original, string prefix)
    {
        var newMessageTemplate = _parser.Parse(prefix + original.MessageTemplate.Text);
        var properties = original.Properties
            .Select(kvp => new LogEventProperty(kvp.Key, kvp.Value))
            .ToList();

        return new LogEvent(
            original.Timestamp,
            original.Level,
            original.Exception,
            newMessageTemplate,
            properties,
            original.TraceId.GetValueOrDefault(),
            original.SpanId.GetValueOrDefault());
    }
}

internal static class TreeConsoleSinkExtensions
{
    internal static LoggerConfiguration TreeConsole(
        this LoggerSinkConfiguration sinkConfiguration,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        string outputTemplate = TreeConsoleWriter.DefaultOutputTemplate)
    {
        var formatter = new MessageTemplateTextFormatter(outputTemplate);
        return sinkConfiguration.Sink(new TreeConsoleSink(formatter), restrictedToMinimumLevel);
    }
}
