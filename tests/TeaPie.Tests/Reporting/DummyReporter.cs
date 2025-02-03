using TeaPie.Reporting;

namespace TeaPie.Tests.Reporting;

public class DummyReporter : IReporter<TestsResultsSummary>
{
    public bool Reported { get; private set; }

    public void Report(TestsResultsSummary report) => Reported = true;
}
