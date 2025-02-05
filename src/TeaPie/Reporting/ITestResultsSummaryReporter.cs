using TeaPie.Testing;

namespace TeaPie.Reporting;

public interface ITestResultsSummaryReporter : ICompositeReporter<IReporter<TestResultsSummary>, TestResultsSummary>
{
    void RegisterTestResult(string testCaseName, TestResult testResult);

    void Initialize();
}
