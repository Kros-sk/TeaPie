namespace TeaPie.Reporting;

internal class InlineTestsResultsSummaryReporter(Action<TestsResultsSummary> reportAction) : IReporter<TestsResultsSummary>
{
    private readonly Action<TestsResultsSummary> _reportAction = reportAction;

    public void Report(TestsResultsSummary report) => _reportAction(report);
}
