using TeaPie.Testing;

namespace TeaPie.Reporting;

public interface ITestsResultsSummaryReporter : ICompositeReporter<IReporter<TestsResultsSummary>, TestsResultsSummary>
{
    void RegisterTestResult(TestResult testResult);

    TestsResultsSummary GetTestResultsSummary();
}
