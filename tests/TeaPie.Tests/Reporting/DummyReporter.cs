using TeaPie.Reporting;
using TeaPie.Testing;

namespace TeaPie.Tests.Reporting;

public class DummyReporter : IReporter<TestResultsSummary>
{
    public bool Reported { get; private set; }

    public async Task Report(TestResultsSummary report)
    {
        Reported = true;
        await Task.CompletedTask;
    }
}
