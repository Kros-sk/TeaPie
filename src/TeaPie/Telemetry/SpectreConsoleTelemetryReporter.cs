using Spectre.Console;
using TeaPie.Reporting;

namespace TeaPie.Telemetry;

/// <summary>
/// Renders a performance summary table to the console using Spectre.Console.
/// </summary>
internal class SpectreConsoleTelemetryReporter : IReporter<TelemetryData>
{
    public Task Report(TelemetryData report)
    {
        if (report.TotalRequests == 0)
        {
            return Task.CompletedTask;
        }

        var table = PrepareMainTable();

        AddHttpSummaryRow(table, report);
        AddRetrySummaryRow(table, report);
        AddPerformanceSummaryRow(table, report);

        table.AddEmptyRow();

        AddHighlightRows(table, report);

        AnsiConsole.Write(table);

        return Task.CompletedTask;
    }

    private static Table PrepareMainTable()
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.Expand();
        table.AddColumn(
            (CompatibilityChecker.SupportsEmoji ? Emoji.Known.BarChart + " " : string.Empty)
            + "[bold yellow]Performance Summary[/]");

        return table;
    }

    private static void AddHttpSummaryRow(Table table, TelemetryData data)
    {
        var emoji = CompatibilityChecker.SupportsEmoji ? Emoji.Known.GlobeWithMeridians + " " : string.Empty;
        var durationRange = FormatDurationRange(data.MinDurationMs, data.MaxDurationMs);

        table.AddEmptyRow();
        table.AddRow(
            $"  {emoji}[bold]HTTP:[/] {data.TotalRequests} requests • " +
            $"{data.SuccessRate:F1}% success rate • " +
            $"{FormatDuration(data.AverageDurationMs)} avg • " +
            $"{durationRange} range");
    }

    private static void AddRetrySummaryRow(Table table, TelemetryData data)
    {
        var emoji = CompatibilityChecker.SupportsEmoji ? Emoji.Known.CounterclockwiseArrowsButton + " " : string.Empty;

        if (data.RequestsWithRetries > 0)
        {
            table.AddRow(
                $"  {emoji}[bold]Retry:[/] {data.TotalRetryAttempts} attempts • " +
                $"{data.RequestsWithRetries} requests ({data.RetryPercentage:F1}%) • " +
                $"{data.RetrySuccessRate:F1}% success rate");
        }
        else
        {
            table.AddRow(
                $"  {emoji}[bold]Retry:[/] No retries needed");
        }
    }

    private static void AddPerformanceSummaryRow(Table table, TelemetryData data)
    {
        var emoji = CompatibilityChecker.SupportsEmoji ? Emoji.Known.Stopwatch + " " : string.Empty;

        table.AddRow(
            $"  {emoji}[bold]Performance:[/] {data.TotalDurationSeconds:F1}s total • " +
            $"{data.Throughput:F2} req/s throughput");
    }

    private static void AddHighlightRows(Table table, TelemetryData data)
    {
        var fastest = data.FastestRequest;
        var slowest = data.SlowestRequest;
        var mostRetried = data.MostRetriedRequest;

        if (fastest is not null)
        {
            var emoji = CompatibilityChecker.SupportsEmoji ? Emoji.Known.Rocket + " " : string.Empty;
            table.AddRow(
                $"  {emoji}[bold green]Fastest:[/] {fastest.Method} {fastest.Url.EscapeMarkup()} ({FormatDuration(fastest.DurationMs)})");
        }

        if (slowest is not null && data.TotalRequests > 1)
        {
            var emoji = CompatibilityChecker.SupportsEmoji ? Emoji.Known.Snail + " " : string.Empty;
            table.AddRow(
                $"  {emoji}[bold red]Slowest:[/] {slowest.Method} {slowest.Url.EscapeMarkup()} ({FormatDuration(slowest.DurationMs)})");
        }

        if (mostRetried is not null)
        {
            var emoji = CompatibilityChecker.SupportsEmoji ? Emoji.Known.CounterclockwiseArrowsButton + " " : string.Empty;
            table.AddRow(
                $"  {emoji}[bold yellow]Most retried:[/] {mostRetried.Method} {mostRetried.Url.EscapeMarkup()} ({mostRetried.RetryAttempts}x)");
        }

        table.AddEmptyRow();
    }

    private static string FormatDuration(double ms)
    {
        if (ms < 1000)
        {
            return $"{ms:F0}ms";
        }

        return $"{ms / 1000:F1}s";
    }

    private static string FormatDurationRange(long min, long max)
        => $"{FormatDuration(min)}-{FormatDuration(max)}";
}
