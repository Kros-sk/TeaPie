using TeaPie.Pipelines;
using TeaPie.Testing;

namespace TeaPie.Reporting;

internal class ReportTestResultsSummaryStep(ITestResultsSummaryAccessor testResultsSummaryAccessor) : IPipelineStep
{
    private readonly ITestResultsSummaryAccessor _testResultsSummaryAccessor = testResultsSummaryAccessor;

    public bool ShouldExecute(ApplicationContext context) => true;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        UpdateContext(context);
        RegisterReporters(context.ReportFilePath, context.Reporter);
        await context.Reporter.Report();
    }

    private void UpdateContext(ApplicationContext context)
    {
        var summary = _testResultsSummaryAccessor.Summary
            ?? throw new InvalidOperationException("Unable to retrieve test results summary.");

        context.AllTestsPassed = summary.AllTestsPassed;
    }

    private static void RegisterReporters(string reportFilePath, ITestResultsSummaryReporter reporter)
    {
        reporter.RegisterReporter(new SpectreConsoleTestResultsSummaryReporter());

        if (!string.IsNullOrEmpty(reportFilePath))
        {
            reporter.RegisterReporter(new JUnitXmlTestResultsSummaryReporter(reportFilePath));
        }
    }
}
