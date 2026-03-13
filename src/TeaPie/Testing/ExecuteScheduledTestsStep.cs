using TeaPie.Logging.Tree;
using TeaPie.Pipelines;
using TeaPie.Reporting;

namespace TeaPie.Testing;

internal class ExecuteScheduledTestsStep(
    ITestScheduler scheduler,
    ITester tester,
    ITestResultsSummaryReporter resultsSummaryReporter) : IPipelineStep
{
    private readonly ITestScheduler _scheduler = scheduler;
    private readonly ITester _tester = tester;
    private readonly ITestResultsSummaryReporter _resultsSummaryReporter = resultsSummaryReporter;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        _resultsSummaryReporter.Initialize();

        using (context.Logger.BeginTreeScope())
        {
            while (_scheduler.HasScheduledTest())
            {
                var test = _scheduler.Dequeue();
                await _tester.ExecuteOrSkipTest(test, test.TestCase);
            }
        }
    }
}
