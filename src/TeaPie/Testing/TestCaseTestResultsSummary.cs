namespace TeaPie.Testing;

public class TestCaseTestResultsSummary(string name)
{
    public string Name { get; } = name;
    public TestResultsSummary Summary { get; } = new();

    internal void AddSkippedTest(TestResult.NotRun skippedTestResult)
        => Summary.AddSkippedTest(skippedTestResult);

    internal void AddPassedTest(TestResult.Passed passedTestResult)
        => Summary.AddPassedTest(passedTestResult);

    internal void AddFailedTest(TestResult.Failed failedTestResult)
        => Summary.AddFailedTest(failedTestResult);
}
