using Spectre.Console;
using System.Text;
using TeaPie.Testing;

namespace TeaPie.Reporting;

internal class SpectreConsoleTestResultsSummaryReporter : IReporter<TestResultsSummary>
{
    public void Report(TestResultsSummary report)
    {
        if (report.NumberOfTests > 0)
        {
            ReportTestResultsSummary(report);
        }
        else
        {
            ReportZeroTests();
        }
    }

    private static void ReportTestResultsSummary(TestResultsSummary summary)
    {
        var table = PrepareMainTable("[bold yellow]Test Results:[/] " + GetOverallResult(summary));

        ReportSkippedTestsIfAny(summary, table);
        ReportFailedTestsIfAny(summary, table);
        ReportSummary(summary, table);

        AnsiConsole.Write(table);
    }

    private static Table PrepareMainTable(string heading)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.Expand();

        table.AddColumn(heading);
        return table;
    }

    private static void ReportSkippedTestsIfAny(TestResultsSummary summary, Table table)
        => ReportTestsGroup(
            table,
            summary.HasSkippedTests,
            (CompatibilityChecker.SupportsEmoji ? Emoji.Known.ThinkingFace + " " : string.Empty) + "Skipped Tests",
            summary.SkippedTests,
            GetTestSkippedText);

    private static void ReportFailedTestsIfAny(TestResultsSummary summary, Table table)
        => ReportTestsGroup(
            table,
            !summary.AllTestsPassed,
            (CompatibilityChecker.SupportsEmoji ? Emoji.Known.CrossMark + " " : string.Empty) + "Failed Tests",
            summary.FailedTests,
            GetTestFailedText);

    private static string GetTestSkippedText(TestResult result)
        => $"[bold orange1]Test skipped: [/][bold yellow]\"{result.TestName.EscapeMarkup()}\"[/]" + Environment.NewLine +
            $"[bold orange1]Test case: [/][italic aqua]{result.TestCasePath.EscapeMarkup()}[/]";

    private static string GetTestFailedText(TestResult result)
        => $"[bold red]Test failed: [/][bold yellow]\"{result.TestName.EscapeMarkup()}\"[/]" + Environment.NewLine +
            $"[bold red]Test case: [/][italic aqua]{result.TestCasePath.EscapeMarkup()}[/]" + Environment.NewLine +
            $"[bold red]Reason: [/][white]{((TestResult.Failed)result).ErrorMessage.EscapeMarkup()}[/]";

    private static void ReportTestsGroup(
        Table table,
        bool condition,
        string sectionName,
        IReadOnlyList<TestResult> testsCollection,
        Func<TestResult, string> textGetter)
    {
        if (condition)
        {
            table.AddEmptyRow();

            var text = BuildTextFromCollectionElements(testsCollection, textGetter);

            AddTestsGroup(table, sectionName, text);
        }
    }

    private static string BuildTextFromCollectionElements(
        IReadOnlyList<TestResult> testsCollection,
        Func<TestResult, string> textGetter)
    {
        var sb = new StringBuilder();

        foreach (var current in testsCollection)
        {
            sb.AppendLine(textGetter(current));

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
        var paddedMarkup = new Padder(markup, new Padding(1, 1, 1, 0));

        var panel = new Panel(paddedMarkup)
            .Border(BoxBorder.Rounded)
            .Header($"[bold aqua] {sectionName.EscapeMarkup()} [/]")
            .Expand();

        table.AddRow(panel);
        table.AddEmptyRow();
    }

    private static void ReportSummary(TestResultsSummary summary, Table table)
    {
        var chart = GetBreakdownChart(summary);
        var panel = GetPanelWithChart(chart);

        table.AddEmptyRow();
        table.AddRow(panel);
        table.AddEmptyRow();
    }

    private static Panel GetPanelWithChart(BreakdownChart chart)
    {
        var paddedChart = new Padder(chart, new Padding(1, 0));

        var panel = new Panel(paddedChart);
        panel.Header("[bold aqua] " +
            (CompatibilityChecker.SupportsEmoji ? Emoji.Known.BarChart + " " : string.Empty) +
            "Summary [/]");

        panel.Expand();
        panel.PadTop(1);
        return panel;
    }

    private static string GetOverallResult(TestResultsSummary summary)
    {
        if (summary.AllTestsPassed)
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

    private static BreakdownChart GetBreakdownChart(TestResultsSummary summary)
        => new BreakdownChart()
            .FullSize()
            .Expand()
            .AddItem(GetPassedTestsTag(summary), summary.NumberOfPassedTests, Color.Green)
            .AddItem(GetSkippedTestsTag(summary), summary.NumberOfSkippedTests, Color.Orange1)
            .AddItem(GetFailedTestsTag(summary), summary.NumberOfFailedTests, Color.Red);

    private static string GetPassedTestsTag(TestResultsSummary summary)
        => $"Passed Tests [{summary.PercentageOfPassedTests:f2}%]:";

    private static string GetSkippedTestsTag(TestResultsSummary summary)
        => $"Skipped Tests [{summary.PercentageOfSkippedTests:f2}%]:";

    private static string GetFailedTestsTag(TestResultsSummary summary)
        => $"Failed Tests [{summary.PercentageOfFailedTests:f2}%]:";

    private static void ReportZeroTests()
    {
        var table = PrepareMainTable(GetZeroTestsReportMessage());
        AnsiConsole.Write(table);
    }

    private static string GetZeroTestsReportMessage()
    {
        const string message = "[bold yellow]Test Results: [/][bold orange1]NO TESTS PERFORMED[/]";
        return CompatibilityChecker.SupportsEmoji ? message + " " + Emoji.Known.ThinkingFace : message;
    }
}
