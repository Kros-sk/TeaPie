namespace TeaPie.Reporting;

internal class InlineTestResultsSummaryReporter(Action<TestResultsSummary> reportAction) : IReporter<TestResultsSummary>
{
    private readonly Action<TestResultsSummary> _reportAction = reportAction;

    public void Report(TestResultsSummary report) => _reportAction(report);
}
