using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace TeaPie.Logging;

internal class TreeConsoleSink(ITextFormatter formatter) : ILogEventSink
{
    private readonly ITextFormatter _formatter = formatter;

    public void Emit(LogEvent logEvent)
    {
        var scopes = TreeScopeStateStore.GetActiveScopes();
        PrintUnopenedScopes(scopes, logEvent);
        var prefix = BuildIndentPrefix(scopes);
        WriteLogEvent(logEvent, prefix);
    }

    private static void PrintUnopenedScopes(IReadOnlyList<TreeScopeStateStore.ScopeState>? scopes, LogEvent logEvent)
    {
        if (scopes == null || scopes.Count == 0)
        {
            return;
        }

        foreach (var s in scopes)
        {
            if (!s.Printed)
            {
                TreeConsoleWriter.WriteOpening(s.Depth, logEvent.Timestamp, TreeConsoleWriter.LevelToShort(logEvent.Level));
                TreeScopeStateStore.MarkPrinted(s);
            }
        }
    }

    private static string BuildIndentPrefix(IReadOnlyList<TreeScopeStateStore.ScopeState>? scopes)
    {
        var printedCount = scopes?.Count(s => s.Printed) ?? 0;
        return TreeConsoleWriter.BuildPrefix(printedCount);
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

internal static class TreeConsoleSinkExtensions
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
