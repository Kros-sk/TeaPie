using TeaPie.Testing;
using TeaPie.Xml;

namespace TeaPie.Reporting;

internal class JUnitXmlTestResultsSummaryReporter(string reportFilePath) : IReporter<TestResultsSummary>
{
    private readonly string _reportFilePath = reportFilePath;

    public void Report(TestResultsSummary report)
    {
        using var writer = new JUnitXmlWriter(_reportFilePath);
        writer.WriteTestSuitesRoot(
            report.NumberOfTests, report.NumberOfSkippedTests, report.NumberOfFailedTests, report.TimeElapsedDuringTesting);
    }
}
