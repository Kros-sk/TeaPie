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
        var scopes = TreeScope.GetActiveScopes();
        var printedCount = PrintUnopenedScopes(scopes, logEvent);
        var totalDepth = printedCount + TreeScope.OuterDepth;
        var prefix = TreeConsoleWriter.BuildPrefix(totalDepth);
        WriteLogEvent(logEvent, prefix);
    }

    private static int PrintUnopenedScopes(IReadOnlyList<TreeScope>? scopes, LogEvent logEvent)
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
                TreeConsoleWriter.WriteOpening(s.Depth + TreeScope.OuterDepth, logEvent.Timestamp, TreeConsoleWriter.LevelToShort(logEvent.Level));
                TreeScope.MarkPrinted(s, logEvent.Level);
            }

            printedCount++;
        }

        return printedCount;
    }

    private void WriteLogEvent(LogEvent logEvent, string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
        {
            _formatter.Format(logEvent, Console.Out);
            return;
        }

        using var sw = new StringWriter();
        _formatter.Format(logEvent, sw);
        TreeConsoleWriter.WriteLogMessage(sw.ToString(), prefix);
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
