using NSubstitute;
using TeaPie.Reporting;
using TeaPie.Testing;
using static Xunit.Assert;
using TestResult = TeaPie.Testing.TestResult;

namespace TeaPie.Tests.Reporting;

public partial class TestResultsSummaryReporterShould
{
    [Fact]
    public async Task NotTriggerReportMethodOnUnregisteredReporter()
    {
        var accessor = new TestResultsSummaryAccessor() { Summary = new() };
        var compositeReporter = new TestResultsSummaryReporter(accessor);
        var reporter1 = Substitute.For<IReporter<TestResultsSummary>>();
        var reporter2 = new DummyReporter();

        compositeReporter.RegisterReporter(reporter1);
        compositeReporter.RegisterReporter(reporter2);

        compositeReporter.UnregisterReporter(reporter2);

        await compositeReporter.Report();

        await reporter1.Received(1).Report(Arg.Any<TestResultsSummary>());
        False(reporter2.Reported);
    }

    [Fact]
    public async Task TriggerReportMethodOnAllRegisteredReporters()
    {
        var accessor = new TestResultsSummaryAccessor() { Summary = new() };
        var compositeReporter = new TestResultsSummaryReporter(accessor);
        var reporter1 = Substitute.For<IReporter<TestResultsSummary>>();
        var reporter2 = new DummyReporter();

        compositeReporter.RegisterReporter(reporter1);
        compositeReporter.RegisterReporter(reporter2);

        await compositeReporter.Report();

        await reporter1.Received(1).Report(Arg.Any<TestResultsSummary>());
        True(reporter2.Reported);
    }

    [Fact]
    public void ChangeTheStateOfSummaryWhenRegisteringTestResults()
    {
        var accessor = new TestResultsSummaryAccessor() { Summary = new CollectionTestResultsSummary() };
        var reporter = new TestResultsSummaryReporter(accessor);
        var skippedTestResult = new TestResult.NotRun() { TestName = "Ignored Test" };
        var passedTestResult = new TestResult.Passed(20) { TestName = "Passed Test" };
        var failedTestResult = new TestResult.Failed(10, "Unknown reason.", null) { TestName = "Failed Test" };

        reporter.Initialize();
        reporter.RegisterTestResult("test-case", skippedTestResult);
        reporter.RegisterTestResult("test-case", passedTestResult);
        reporter.RegisterTestResult("test-case", failedTestResult);

        CheckSummary(skippedTestResult, failedTestResult, accessor.Summary);
    }

    [Fact]
    public void ThrowWhenStartIsCalledTwiceOnSummary()
    {
        var summary = new CollectionTestResultsSummary();
        var accessor = new TestResultsSummaryAccessor() { Summary = summary };
        var reporter = new TestResultsSummaryReporter(accessor);

        reporter.Initialize();
        var originalTimestamp = summary.Timestamp;

        Thread.Sleep(15);

        reporter.Initialize();

        Equal(originalTimestamp, summary.Timestamp);
    }

    private static void CheckSummary(
        TestResult.NotRun skippedTestResult,
        TestResult.Failed failedTestResult,
        TestResultsSummary summary)
    {
        False(summary.AllTestsPassed);
        True(summary.HasSkippedTests);

        Equal(1, summary.NumberOfPassedTests);
        Equal(1, summary.NumberOfFailedTests);
        Equal(1, summary.NumberOfSkippedTests);

        True(Math.Abs(33.33 - summary.PercentageOfPassedTests) < 0.02);
        True(Math.Abs(33.33 - summary.PercentageOfFailedTests) < 0.02);
        True(Math.Abs(33.33 - summary.PercentageOfSkippedTests) < 0.02);

        Equal(2, summary.NumberOfExecutedTests);
        Equal(3, summary.NumberOfTests);

        Equal(30, summary.TimeElapsedDuringTesting);

        Contains(skippedTestResult, summary.SkippedTests);
        Single(summary.SkippedTests);

        Contains(failedTestResult, summary.FailedTests);
        Single(summary.FailedTests);
    }
}
