using TeaPie.Testing;

namespace TeaPie.Reporting;

internal class TestResultsSummary
{
    public bool AllTestsPassed => NumberOfPassedTests == NumberOfExecutedTests;

    public int NumberOfSkippedTests { get; private set; }
    public int NumberOfPassedTests { get; private set; }
    public int NumberOfFailedTests { get; private set; }
    public double TimeElapsedDuringTesting { get; private set; }

    public int NumberOfTests => NumberOfSkippedTests + NumberOfPassedTests + NumberOfFailedTests;
    public int NumberOfExecutedTests => NumberOfPassedTests + NumberOfFailedTests;

    public double PercentageOfSkippedTests => (double)NumberOfSkippedTests / NumberOfTests * 100;
    public double PercentageOfPassedTests => (double)NumberOfPassedTests / NumberOfTests * 100;
    public double PercentageOfFailedTests => (double)NumberOfFailedTests / NumberOfTests * 100;

    public void AddSkippedTest() => NumberOfSkippedTests++;

    public void AddPassedTest(TestResult.Passed passedTestResult)
    {
        NumberOfPassedTests++;
        TimeElapsedDuringTesting += passedTestResult.Duration;
    }

    public void AddFailedTest(TestResult.Failed failedTestResult)
    {
        NumberOfFailedTests++;
        TimeElapsedDuringTesting += failedTestResult.Duration;
        _failedTests.Add(failedTestResult);
    }

    private readonly List<TestResult.Failed> _failedTests = [];

    public IReadOnlyList<TestResult.Failed> FailedTests => _failedTests;
}
