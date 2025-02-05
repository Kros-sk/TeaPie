using TeaPie.Testing;

namespace TeaPie.Reporting;

internal class CollectionTestResultsSummaryReporter : ITestResultsSummaryReporter
{
    private readonly List<IReporter<TestResultsSummary>> _reporters = [];
    private CollectionTestResultsSummary _summary = new();
    private bool _started;

    public void RegisterReporter(IReporter<TestResultsSummary> reporter) => _reporters.Add(reporter);
    public void UnregisterReporter(IReporter<TestResultsSummary> reporter) => _reporters.Remove(reporter);

    public void Start(string name)
    {
        _summary = new(name);
        _summary.Start();
        _started = true;
    }

    public void RegisterTestResult(string testCaseName, TestResult testResult)
    {
        if (!_started)
        {
            throw new InvalidOperationException("Unable to register test result, if collection run didn't start.");
        }

        switch (testResult)
        {
            case TestResult.NotRun skipped: _summary.AddSkippedTest(testCaseName, skipped); break;
            case TestResult.Passed passed: _summary.AddPassedTest(testCaseName, passed); break;
            case TestResult.Failed failed: _summary.AddFailedTest(testCaseName, failed); break;
        }
    }

    public void Report()
    {
        foreach (var reporter in _reporters)
        {
            reporter.Report(_summary);
        }
    }

    public CollectionTestResultsSummary GetSummary() => _summary;
}
