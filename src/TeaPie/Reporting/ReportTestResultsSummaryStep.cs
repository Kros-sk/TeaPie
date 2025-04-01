using TeaPie.Pipelines;

namespace TeaPie.Reporting;

internal class ReportTestResultsSummaryStep : IPipelineStep
{
    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        RegisterReporters(context.ReportFilePath, context.Reporter);
        context.Reporter.Report();

        await Task.CompletedTask;
    }

    public bool ShouldExecute(ApplicationContext context) => true;

    private static void RegisterReporters(string reportFilePath, ITestResultsSummaryReporter reporter)
    {
        reporter.RegisterReporter(new SpectreConsoleTestResultsSummaryReporter());

        if (!string.IsNullOrEmpty(reportFilePath))
        {
            reporter.RegisterReporter(new JUnitXmlTestResultsSummaryReporter(reportFilePath));
        }
    }
}
