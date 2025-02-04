using NSubstitute;
using TeaPie.Reporting;
using static Xunit.Assert;
using TestResult = TeaPie.Testing.TestResult;

namespace TeaPie.Tests.Reporting;

public partial class TestResultsSummaryReporterShould
{
    [Fact]
    public void NotTriggerReportMethodOnUnregisteredReporter()
    {
        var compositeReporter = new TestResultsSummaryReporter();
        var reporter1 = Substitute.For<IReporter<TestResultsSummary>>();
        var reporter2 = new DummyReporter();

        compositeReporter.RegisterReporter(reporter1);
        compositeReporter.RegisterReporter(reporter2);

        compositeReporter.UnregisterReporter(reporter2);

        compositeReporter.Report();

        reporter1.Received(1).Report(Arg.Any<TestResultsSummary>());
        False(reporter2.Reported);
    }

    [Fact]
    public void TriggerReportMethodOnAllRegisteredReporters()
    {
        var compositeReporter = new TestResultsSummaryReporter();
        var reporter1 = Substitute.For<IReporter<TestResultsSummary>>();
        var reporter2 = new DummyReporter();

        compositeReporter.RegisterReporter(reporter1);
        compositeReporter.RegisterReporter(reporter2);

        compositeReporter.Report();

        reporter1.Received(1).Report(Arg.Any<TestResultsSummary>());
        True(reporter2.Reported);
    }

    [Fact]
    public void ChangeTheStateOfSummaryWhenRegisteringTestResults()
    {
        var reporter = new TestResultsSummaryReporter();
        var skippedTestResult = new TestResult.NotRun() { TestName = "Ignored Test" };
        var passedTestResult = new TestResult.Passed(20) { TestName = "Passed Test" };
        var failedTestResult = new TestResult.Failed(10, "Unknown reason.", null) { TestName = "Failed Test" };

        reporter.RegisterTestResult(skippedTestResult);
        reporter.RegisterTestResult(passedTestResult);
        reporter.RegisterTestResult(failedTestResult);

        var summary = reporter.GetTestResultsSummary();

        CheckSummary(skippedTestResult, failedTestResult, summary);
    }

    [Fact]
    public void ResetTheStateOfSummaryWhenResetMethodIsCalled()
    {
        var reporter = new TestResultsSummaryReporter();
        var skippedTestResult = new TestResult.NotRun() { TestName = "Ignored Test" };
        var passedTestResult = new TestResult.Passed(20) { TestName = "Passed Test" };
        var failedTestResult = new TestResult.Failed(10, "Unknown reason.", null) { TestName = "Failed Test" };

        reporter.RegisterTestResult(skippedTestResult);
        reporter.RegisterTestResult(passedTestResult);
        reporter.RegisterTestResult(failedTestResult);

        reporter.Reset();
        var emptySummary = reporter.GetTestResultsSummary();

        CheckEmptySummary(emptySummary);
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

    private static void CheckEmptySummary(TestResultsSummary summary)
    {
        True(summary.AllTestsPassed);
        False(summary.HasSkippedTests);

        Equal(0, summary.NumberOfPassedTests);
        Equal(0, summary.NumberOfFailedTests);
        Equal(0, summary.NumberOfSkippedTests);

        Equal(0, summary.PercentageOfPassedTests);
        Equal(0, summary.PercentageOfFailedTests);
        Equal(0, summary.PercentageOfSkippedTests);

        Equal(0, summary.NumberOfExecutedTests);
        Equal(0, summary.NumberOfTests);

        Equal(0, summary.TimeElapsedDuringTesting);

        Empty(summary.SkippedTests);
        Empty(summary.FailedTests);
    }
}
