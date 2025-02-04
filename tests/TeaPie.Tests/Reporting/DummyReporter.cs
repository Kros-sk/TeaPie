using TeaPie.Reporting;
using TeaPie.Testing;

namespace TeaPie.Tests.Reporting;

public class DummyReporter : IReporter<TestResultsSummary>
{
    public bool Reported { get; private set; }

    public void Report(TestResultsSummary report) => Reported = true;
}
