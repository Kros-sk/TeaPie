using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Http.Retrying;
using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.TestCases;
using TeaPie.Testing;
using TeaPie.Variables;
using static Xunit.Assert;

namespace TeaPie.Tests.Reporting;

[Collection(nameof(NonParallelCollection))]
public class TeaPieReportingExtensionsShould
{
    [Fact]
    public void RegisterReporterCorrectly()
    {
        var accessor = new TestResultsSummaryAccessor() { Summary = new() };
        var reporter = new CollectionTestResultsSummaryReporter(accessor);
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
        var reporter = new CollectionTestResultsSummaryReporter(accessor);
        var teaPie = PrepareTeaPieInstance(reporter);
        var reported = false;

        teaPie.RegisterReporter(_ => reported = true);

        reporter.Report();

        True(reported);
    }

    private static TeaPie PrepareTeaPieInstance(CollectionTestResultsSummaryReporter reporter)
        => TeaPie.Create(
            Substitute.For<IVariables>(),
            Substitute.For<ILogger>(),
            Substitute.For<ITester>(),
            Substitute.For<ICurrentTestCaseExecutionContextAccessor>(),
            new ApplicationContextBuilder().Build(),
            Substitute.For<IPipeline>(),
            reporter,
            Substitute.For<IRetryingPoliciesRegistry>());
}
