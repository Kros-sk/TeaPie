using TeaPie.Testing;

namespace TeaPie.Reporting;

internal class InlineTestResultsSummaryReporter(Func<TestResultsSummary, Task> reportAction) : IReporter<TestResultsSummary>
{
    private readonly Func<TestResultsSummary, Task> _reportAction = reportAction;

    public Task Report(TestResultsSummary report) => _reportAction(report);
}
