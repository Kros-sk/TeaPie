using Spectre.Console;
using System.Text;
using TeaPie.Testing;

namespace TeaPie.Reporting;

internal class SpectreConsoleTestCaseSummaryReporter : IReporter<TestResultsSummary>
{
    public void Report(TestResultsSummary report)
    {
        var table = PrepareMainTable(report);

        ReportSkippedTestsIfAny(report, table);
        ReportFailedTestsIfAny(report, table);
        ReportSummary(report, table);

        AnsiConsole.Write(table);
    }

    private static Table PrepareMainTable(TestResultsSummary summary)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);

        table.AddColumn("[bold yellow]Test Results:[/] " + GetOverallResult(summary));
        return table;
    }

    private static void ReportSkippedTestsIfAny(TestResultsSummary summary, Table table)
        => ReportTestsGroup(
            table,
            summary.HasSkippedTests,
            (CompatibilityChecker.SupportsEmoji ? Emoji.Known.ThinkingFace + " " : string.Empty) + "Skipped Tests",
            summary.SkippedTests,
            (result) => $"[bold orange1]Test skipped: [/][bold yellow]\"{result.TestName}\"[/]");

    private static void ReportFailedTestsIfAny(TestResultsSummary summary, Table table)
        => ReportTestsGroup(
            table,
            !summary.AllTestsPassed,
            (CompatibilityChecker.SupportsEmoji ? Emoji.Known.CrossMark + " " : string.Empty) + "Failed Tests",
            summary.FailedTests,
            (result) => $"[bold red]Test failed: [/][bold yellow]\"{result.TestName}\"[/]",
            (result) => $"[bold red]Reason: [/][white]{((TestResult.Failed)result).ErrorMessage}[/]");

    private static void ReportTestsGroup(
        Table table,
        bool condition,
        string sectionName,
        IReadOnlyList<TestResult> testsCollection,
        Func<TestResult, string> firstLineGetter,
        Func<TestResult, string>? secondLineGetter = null)
    {
        if (condition)
        {
            table.AddEmptyRow();

            var text = BuildTheTextFromCollectionElements(testsCollection, firstLineGetter, secondLineGetter);

            AddTestsGroup(table, sectionName, text);
        }
    }

    private static string BuildTheTextFromCollectionElements(
        IReadOnlyList<TestResult> testsCollection,
        Func<TestResult, string> firstLineGetter,
        Func<TestResult, string>? secondLineGetter)
    {
        var sb = new StringBuilder();

        foreach (var current in testsCollection)
        {
            sb.AppendLine(firstLineGetter(current));
            if (secondLineGetter is not null)
            {
                sb.AppendLine(secondLineGetter(current));
            }

            if (current != testsCollection[^1])
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static void AddTestsGroup(Table table, string sectionName, string text)
    {
        var markup = new Markup(text).LeftJustified();
        var paddedMarkup = new Padder(markup, new Padding(1, 1, 0, 0));

        var panel = new Panel(paddedMarkup)
            .Border(BoxBorder.Rounded)
            .Header($"[bold aqua] {sectionName} [/]")
            .Expand();

        table.AddRow(panel);
        table.AddEmptyRow();
    }

    private static void ReportSummary(TestResultsSummary summary, Table table)
    {
        var chart = GetBreakdownChart(summary);
        var panel = new Panel(chart);
        panel.Header("[bold aqua] " +
            (CompatibilityChecker.SupportsEmoji ? Emoji.Known.BarChart + " " : string.Empty) +
            "Summary [/]");

        panel.Expand();
        panel.PadTop(1);

        table.AddEmptyRow();
        table.AddRow(panel);
        table.AddEmptyRow();
    }

    private static string GetOverallResult(TestResultsSummary report)
    {
        if (report.AllTestsPassed)
        {
            const string message = "[bold green]SUCCESS[/]";
            return CompatibilityChecker.SupportsEmoji ? message + " " + Emoji.Known.CheckMarkButton : message;
        }
        else
        {
            const string message = "[bold red]FAILED[/]";
            return CompatibilityChecker.SupportsEmoji ? message + " " + Emoji.Known.CrossMark : message;
        }
    }

    private static BreakdownChart GetBreakdownChart(TestResultsSummary report)
        => new BreakdownChart()
            .FullSize()
            .Expand()
            .AddItem(GetPassedTestsTag(report), report.NumberOfPassedTests, Color.Green)
            .AddItem(GetSkippedTestsTag(report), report.NumberOfSkippedTests, Color.Orange1)
            .AddItem(GetFailedTestsTag(report), report.NumberOfFailedTests, Color.Red);

    private static string GetPassedTestsTag(TestResultsSummary report)
        => $"Passed Tests [{report.PercentageOfPassedTests:f2}%]:";

    private static string GetSkippedTestsTag(TestResultsSummary report)
        => $"Skipped Tests [{report.PercentageOfSkippedTests:f2}%]:";

    private static string GetFailedTestsTag(TestResultsSummary report)
        => $"Failed Tests [{report.PercentageOfFailedTests:f2}%]:";
}
