using TeaPie.Pipelines;

namespace TeaPie.Reporting;

internal class ReportTestResultsSummaryStep : IPipelineStep
{
    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        RegisterAllAvailableReporters(context.Reporter);
        context.Reporter.Report();

        await Task.CompletedTask;
    }

    private static void RegisterAllAvailableReporters(ITestResultsSummaryReporter reporter)
    {
        reporter.RegisterReporter(new SpectreConsoleTestCaseSummaryReporter());
    }
}
