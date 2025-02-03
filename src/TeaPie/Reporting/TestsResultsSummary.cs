using TeaPie.Testing;

namespace TeaPie.Reporting;

public class TestsResultsSummary
{
    public bool AllTestsPassed => NumberOfPassedTests == NumberOfExecutedTests;
    public bool HasSkippedTests => NumberOfSkippedTests > 0;

    public int NumberOfSkippedTests { get; private set; }
    public int NumberOfPassedTests { get; private set; }
    public int NumberOfFailedTests { get; private set; }
    public double TimeElapsedDuringTesting { get; private set; }

    public int NumberOfTests => NumberOfSkippedTests + NumberOfPassedTests + NumberOfFailedTests;
    public int NumberOfExecutedTests => NumberOfPassedTests + NumberOfFailedTests;

    public double PercentageOfSkippedTests
        => NumberOfTests > 0 ? (double)NumberOfSkippedTests / NumberOfTests * 100 : 0.00;

    public double PercentageOfPassedTests
        => NumberOfTests > 0 ? (double)NumberOfPassedTests / NumberOfTests * 100 : 0.00;

    public double PercentageOfFailedTests
        => NumberOfTests > 0 ? (double)NumberOfFailedTests / NumberOfTests * 100 : 0.00;

    private readonly List<TestResult.NotRun> _skippedTests = [];
    public IReadOnlyList<TestResult.NotRun> SkippedTests => _skippedTests;

    private readonly List<TestResult.Failed> _failedTests = [];
    public IReadOnlyList<TestResult.Failed> FailedTests => _failedTests;

    internal void AddSkippedTest(TestResult.NotRun skippedTestResult)
    {
        NumberOfSkippedTests++;
        _skippedTests.Add(skippedTestResult);
    }

    internal void AddPassedTest(TestResult.Passed passedTestResult)
    {
        NumberOfPassedTests++;
        TimeElapsedDuringTesting += passedTestResult.Duration;
    }

    internal void AddFailedTest(TestResult.Failed failedTestResult)
    {
        NumberOfFailedTests++;
        TimeElapsedDuringTesting += failedTestResult.Duration;
        _failedTests.Add(failedTestResult);
    }
}
