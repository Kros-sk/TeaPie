using Spectre.Console;

namespace TeaPie.Reporting;

public class SpectreConsoleReporter : IReporter
{
    public void ReportTestStart(string testName)
    {
        AnsiConsole.MarkupLine($"[yellow]Running test:[/] {testName}");
    }

    public void ReportTestSuccess(string testName)
    {
        AnsiConsole.MarkupLine($"[green]Test Passed:[/] {testName}");
    }

    public void ReportTestFailure(string testName, string errorMessage)
    {
        AnsiConsole.MarkupLine($"[red]Test Failed:[/] {testName}");
        AnsiConsole.MarkupLine($"[red]Error:[/] {errorMessage}");
    }

    public void ReportTestCaseSummary(int passed, int failed, int skipped)
    {
        AnsiConsole.MarkupLine("[bold]Test Summary:[/]");
        AnsiConsole.MarkupLine($"[green]Passed:[/] {passed}");
        AnsiConsole.MarkupLine($"[red]Failed:[/] {failed}");
        AnsiConsole.MarkupLine($"[yellow]Skipped:[/] {skipped}");
    }
}
