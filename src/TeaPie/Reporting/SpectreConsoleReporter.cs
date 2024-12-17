using Spectre.Console;

namespace TeaPie.Reporting;

public class SpectreConsoleReporter : IReporter
{
    public void ReportTestStart(string testName, string path)
    {
        AnsiConsole.MarkupLine($"[yellow]Running test:[/][white] {testName} [/][i][gray]({path})[/][/]");
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
}
