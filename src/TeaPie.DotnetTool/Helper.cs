using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace TeaPie.DotnetTool;

internal static class Helper
{
    public static LogLevel ResolveLogLevel(LoggingSettings settings)
        => settings switch
        {
            { IsQuiet: true } => Silence(),
            { IsDebug: true } => LogLevel.Debug,
            { IsVerbose: true } => LogLevel.Trace,
            _ => settings.LogLevel
        };

    private static LogLevel Silence()
    {
        Console.SetOut(TextWriter.Null);
        Console.SetError(TextWriter.Null);

        AnsiConsole.Console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(TextWriter.Null)
        });

        return LogLevel.None;
    }
}
