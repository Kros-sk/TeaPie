using TeaPie.Reporting;
using TeaPie.Testing;
using static Xunit.Assert;

namespace TeaPie.Tests.Reporting;

[Collection(nameof(NonParallelCollection))]
public class TeaPieReportingExtensionsShould
{
    [Fact]
    public void RegisterReporterCorrectly()
    {
        var accessor = new TestResultsSummaryAccessor() { Summary = new() };
        var reporter = new TestResultsSummaryReporter(accessor);
        var teaPie = PrepareTeaPieInstance(reporter);
        var dummyReporter = new DummyReporter();

        teaPie.RegisterReporter(dummyReporter);

        reporter.Report();

        True(dummyReporter.Reported);
    }

    [Fact]
    public void RegisterInlineReporterCorrectly()
    {
        var accessor = new TestResultsSummaryAccessor() { Summary = new() };
        var reporter = new TestResultsSummaryReporter(accessor);
        var teaPie = PrepareTeaPieInstance(reporter);
        var reported = false;

        teaPie.RegisterReporter(_ => reported = true);

        reporter.Report();

        True(reported);
    }

    private static TeaPie PrepareTeaPieInstance(TestResultsSummaryReporter reporter)
        => new TeaPieBuilder().WithService<ITestResultsSummaryReporter>(reporter).Build();
}
