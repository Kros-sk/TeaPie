using Spectre.Console;

namespace TeaPie.Reporting;

public class SpectreConsoleReporter : IReporter
{
    public void ReportTestStart(string testName, string path)
    {
        AnsiConsole.MarkupLine($"[yellow]Running test:[/][white] {testName} [/][i][gray]({path})[/][/]");
    }

    public void ReportTestSuccess(string testName, long duration)
    {
        AnsiConsole.MarkupLine($"[green]Test Passed:[/] {testName} [white] in [/][green]{FormatDuration(duration)}[/]");
    }

    public void ReportTestFailure(string testName, string errorMessage, long duration)
    {
        AnsiConsole.MarkupLine($"[red]Test Failed:[/] {testName} [white] after [/][red]{FormatDuration(duration)}[/]");
        AnsiConsole.MarkupLine($"[red]Error:[/] {errorMessage}");
    }

    public static string FormatDuration(long milliseconds)
    {
        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);

        if (timeSpan.TotalSeconds < 1)
        {
            return $"{milliseconds} ms";
        }

        if (timeSpan.TotalSeconds < 60)
        {
            return $"{timeSpan.TotalSeconds:F2} s";
        }

        if (timeSpan.TotalMinutes < 60)
        {
            return $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds}s";
        }

        return $"{(int)timeSpan.TotalHours}h {(int)timeSpan.Minutes}m {timeSpan.Seconds}s";
    }
}
