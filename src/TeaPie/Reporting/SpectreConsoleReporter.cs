using Spectre.Console;

namespace TeaPie.Reporting;

public class SpectreConsoleReporter : IReporter
{
    public void ReportTestStart(string testName, string path)
    {
        AnsiConsole.MarkupLine($"[yellow]Running test:[/][white] {testName.EscapeMarkup()} [/]" +
            $"[i][gray]({path.EscapeMarkup()})[/][/]");
    }

    public void ReportTestSuccess(string testName, long duration)
    {
        AnsiConsole.MarkupLine($"[green]Test Passed:[/] {testName.EscapeMarkup()} [white] in [/]" +
            $"[green]{FormatDuration(duration)}[/]");
    }

    public void ReportTestFailure(string testName, string errorMessage, long duration)
    {
        AnsiConsole.MarkupLine($"[red]Test Failed:[/] {testName.EscapeMarkup()} [white] after [/]" +
            $"[red]{FormatDuration(duration)}[/]");
        AnsiConsole.MarkupLine($"[red]Error:[/] {errorMessage.EscapeMarkup()}");
    }

    public static string FormatDuration(long milliseconds)
    {
        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);

        return timeSpan switch
        {
            { TotalSeconds: < 1 } => $"{milliseconds} ms",
            { TotalSeconds: < 60 } => $"{timeSpan.TotalSeconds:F2} s",
            { TotalMinutes: < 60 } => $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds}s",
            _ => $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m {timeSpan.Seconds}s"
        };
    }
}
