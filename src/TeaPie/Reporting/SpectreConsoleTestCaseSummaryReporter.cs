using Spectre.Console;

namespace TeaPie.Reporting;

internal class SpectreConsoleTestCaseSummaryReporter : IReporter<TestResultsSummary>
{
    public void Report(TestResultsSummary report)
    {
        var chart = GetBreakdownChart(report);

        var table = new Table();
        table.Border(TableBorder.Rounded);

        table.AddColumn("[bold yellow]Test Results:[/] " + GetResult(report));
        table.AddRow(chart);

        ReportErrorsIfNeeded(report, table);

        AnsiConsole.Write(table);
    }

    private static void ReportErrorsIfNeeded(TestResultsSummary report, Table table)
    {
        if (!report.AllTestsPassed)
        {
            table.AddEmptyRow();

            foreach (var error in report.FailedTests)
            {
                table.AddRow($"[bold red]Test failed: [/][bold yellow]\"{error.TestName}\"[/]{Environment.NewLine}" +
                    $"[bold red]Reason: [/][white]{error.ErrorMessage}[/]");
                if (error != report.FailedTests[report.FailedTests.Count - 1])
                {
                    table.AddEmptyRow();
                }
            }
        }
    }

    private static string GetResult(TestResultsSummary report)
    {
        var supportsEmoji = CompatibilityChecker.SupportsEmojis();

        if (report.AllTestsPassed)
        {
            const string message = "[bold green]SUCCESS[/]";
            return supportsEmoji ? Emoji.Known.CheckMarkButton + " " + message : message;
        }
        else
        {
            const string message = "[bold red]FAILED[/]";
            return supportsEmoji ? Emoji.Known.DisappointedFace + " " + message : message;
        }
    }

    private static BreakdownChart GetBreakdownChart(TestResultsSummary report)
        => new BreakdownChart()
            .FullSize()
            .AddItem("Passed Tests", report.NumberOfPassedTests, Color.Green)
            .AddItem("Skipped Tests", report.NumberOfSkippedTests, Color.Orange1)
            .AddItem("Failed Tests", report.NumberOfFailedTests, Color.Red);
}
