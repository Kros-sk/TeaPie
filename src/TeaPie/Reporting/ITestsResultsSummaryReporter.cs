using TeaPie.Testing;

namespace TeaPie.Reporting;

internal interface ITestsResultsSummaryReporter : ICompositeReporter<IReporter<TestResultsSummary>, TestResultsSummary>
{
    void RegisterTestResult(TestResult testResult);

    TestResultsSummary GetTestResultsSummary();
}
