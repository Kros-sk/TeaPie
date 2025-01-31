using TeaPie.Testing;

namespace TeaPie.Reporting;

internal class TestsResultsSummaryReporter : ITestsResultsSummaryReporter
{
    private readonly List<IReporter<TestResultsSummary>> _reporters = [];
    private TestResultsSummary _summary = new();

    public void RegisterReporter(IReporter<TestResultsSummary> reporter) => _reporters.Add(reporter);
    public void UnregisterReporter(IReporter<TestResultsSummary> reporter) => _reporters.Remove(reporter);

    public void RegisterTestResult(TestResult testResult)
    {
        switch (testResult)
        {
            case TestResult.NotRun skipped: _summary.AddSkippedTest(skipped); break;
            case TestResult.Passed passed: _summary.AddPassedTest(passed); break;
            case TestResult.Failed failed: _summary.AddFailedTest(failed); break;
        }
    }

    public void Report()
    {
        foreach (var reporter in _reporters)
        {
            reporter.Report(_summary);
        }
    }

    public void Reset() => _summary = new();

    public TestResultsSummary GetTestResultsSummary() => _summary;
}
