using TeaPie.Testing;

namespace TeaPie.Reporting;

internal class TestResultsSummaryReporter(ITestResultsSummaryAccessor accessor) : ITestResultsSummaryReporter
{
    private readonly List<IReporter<TestResultsSummary>> _reporters = [];
    private readonly ITestResultsSummaryAccessor _accessor = accessor;
    private CollectionTestResultsSummary _summary = new();
    private bool _started;

    public void RegisterReporter(IReporter<TestResultsSummary> reporter) => _reporters.Add(reporter);

    public void UnregisterReporter(IReporter<TestResultsSummary> reporter) => _reporters.Remove(reporter);

    public void Initialize()
    {
        if (_started)
        {
            return;
        }

        _summary = GetSummary();
        _summary.Start();
        _started = true;
    }

    public void RegisterTestResult(string testCaseName, TestResult testResult)
    {
        CheckCurrentState();

        switch (testResult)
        {
            case TestResult.NotRun skipped: _summary.AddSkippedTest(testCaseName, skipped); break;
            case TestResult.Passed passed: _summary.AddPassedTest(testCaseName, passed); break;
            case TestResult.Failed failed: _summary.AddFailedTest(testCaseName, failed); break;
        }
    }

    public async Task Report()
    {
        foreach (var reporter in _reporters)
        {
            await reporter.Report(_summary);
        }
    }

    private void CheckCurrentState()
    {
        if (!_started)
        {
            throw new InvalidOperationException("Unable to register test result, if collection run didn't start yet.");
        }

        if (_summary is null)
        {
            throw new InvalidOperationException("Unable to register test result, if there is no summary object.");
        }
    }

    private CollectionTestResultsSummary GetSummary()
    {
        if (_accessor.Summary is null)
        {
            throw new InvalidOperationException("Unable to start collection run, if no summary object is available.");
        }

        if (_accessor.Summary is not CollectionTestResultsSummary summary)
        {
            throw new InvalidOperationException("Collection reporter has to work with collecton test results summary.");
        }

        return summary;
    }
}
