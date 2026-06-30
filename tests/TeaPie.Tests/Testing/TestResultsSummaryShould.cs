using FluentAssertions;
using TeaPie.Testing;

namespace TeaPie.Tests.Testing;

public class TestResultsSummaryShould
{
    [Fact]
    public void SetTimestampOnStart()
    {
        var summary = new TestResultsSummary();
        var before = DateTime.Now;

        summary.Start();

        summary.Timestamp.Should().BeOnOrAfter(before);
        summary.Timestamp.Should().BeOnOrBefore(DateTime.Now);
    }

    [Fact]
    public void IncrementSkippedCountAndAddToCollections()
    {
        var summary = new TestResultsSummary();
        var skipped = new TestResult.NotRun() { TestName = "skipped1", TestCasePath = "" };

        summary.AddSkippedTest(skipped);

        summary.NumberOfSkippedTests.Should().Be(1);
        summary.SkippedTests.Should().ContainSingle().Which.Should().Be(skipped);
        summary.TestResults.Should().ContainSingle().Which.Should().Be(skipped);
    }

    [Fact]
    public void IncrementPassedCountAndAddDurationAndCollections()
    {
        var summary = new TestResultsSummary();
        var passed = new TestResult.Passed(100) { TestName = "passed1", TestCasePath = "" };

        summary.AddPassedTest(passed);

        summary.NumberOfPassedTests.Should().Be(1);
        summary.TimeElapsedDuringTesting.Should().Be(100);
        summary.PassedTests.Should().ContainSingle().Which.Should().Be(passed);
        summary.TestResults.Should().ContainSingle().Which.Should().Be(passed);
    }

    [Fact]
    public void IncrementFailedCountAndAddDurationAndCollections()
    {
        var summary = new TestResultsSummary();
        var failed = new TestResult.Failed(200, "error", null) { TestName = "failed1", TestCasePath = "" };

        summary.AddFailedTest(failed);

        summary.NumberOfFailedTests.Should().Be(1);
        summary.TimeElapsedDuringTesting.Should().Be(200);
        summary.FailedTests.Should().ContainSingle().Which.Should().Be(failed);
        summary.TestResults.Should().ContainSingle().Which.Should().Be(failed);
    }

    [Fact]
    public void ReturnSumOfAllTestsForNumberOfTests()
    {
        var summary = new TestResultsSummary();
        summary.AddSkippedTest(new TestResult.NotRun() { TestName = "s1", TestCasePath = "" });
        summary.AddPassedTest(new TestResult.Passed(10) { TestName = "p1", TestCasePath = "" });
        summary.AddFailedTest(new TestResult.Failed(20, "err", null) { TestName = "f1", TestCasePath = "" });

        summary.NumberOfTests.Should().Be(3);
    }

    [Fact]
    public void ReturnPassedPlusFailedForNumberOfExecutedTests()
    {
        var summary = new TestResultsSummary();
        summary.AddSkippedTest(new TestResult.NotRun() { TestName = "s1", TestCasePath = "" });
        summary.AddPassedTest(new TestResult.Passed(10) { TestName = "p1", TestCasePath = "" });
        summary.AddFailedTest(new TestResult.Failed(20, "err", null) { TestName = "f1", TestCasePath = "" });

        summary.NumberOfExecutedTests.Should().Be(2);
    }

    [Fact]
    public void ReturnTrueForAllTestsPassedWhenOnlyPassedTests()
    {
        var summary = new TestResultsSummary();
        summary.AddPassedTest(new TestResult.Passed(10) { TestName = "p1", TestCasePath = "" });
        summary.AddPassedTest(new TestResult.Passed(20) { TestName = "p2", TestCasePath = "" });

        summary.AllTestsPassed.Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseForAllTestsPassedWhenThereAreFailedTests()
    {
        var summary = new TestResultsSummary();
        summary.AddPassedTest(new TestResult.Passed(10) { TestName = "p1", TestCasePath = "" });
        summary.AddFailedTest(new TestResult.Failed(20, "err", null) { TestName = "f1", TestCasePath = "" });

        summary.AllTestsPassed.Should().BeFalse();
    }

    [Fact]
    public void ReturnTrueForHasSkippedTestsWhenSkippedGreaterThanZero()
    {
        var summary = new TestResultsSummary();
        summary.AddSkippedTest(new TestResult.NotRun() { TestName = "s1", TestCasePath = "" });

        summary.HasSkippedTests.Should().BeTrue();
    }

    [Fact]
    public void ReturnFalseForHasSkippedTestsWhenZeroSkipped()
    {
        var summary = new TestResultsSummary();
        summary.AddPassedTest(new TestResult.Passed(10) { TestName = "p1", TestCasePath = "" });

        summary.HasSkippedTests.Should().BeFalse();
    }

    [Fact]
    public void CalculatePercentageOfPassedTestsCorrectly()
    {
        var summary = new TestResultsSummary();
        summary.AddPassedTest(new TestResult.Passed(10) { TestName = "p1", TestCasePath = "" });
        summary.AddFailedTest(new TestResult.Failed(20, "err", null) { TestName = "f1", TestCasePath = "" });

        summary.PercentageOfPassedTests.Should().Be(50.0);
    }

    [Fact]
    public void CalculatePercentageOfFailedTestsCorrectly()
    {
        var summary = new TestResultsSummary();
        summary.AddPassedTest(new TestResult.Passed(10) { TestName = "p1", TestCasePath = "" });
        summary.AddFailedTest(new TestResult.Failed(20, "err", null) { TestName = "f1", TestCasePath = "" });

        summary.PercentageOfFailedTests.Should().Be(50.0);
    }

    [Fact]
    public void CalculatePercentageOfSkippedTestsCorrectly()
    {
        var summary = new TestResultsSummary();
        summary.AddSkippedTest(new TestResult.NotRun() { TestName = "s1", TestCasePath = "" });
        summary.AddPassedTest(new TestResult.Passed(10) { TestName = "p1", TestCasePath = "" });
        summary.AddFailedTest(new TestResult.Failed(20, "err", null) { TestName = "f1", TestCasePath = "" });

        summary.PercentageOfSkippedTests.Should().BeApproximately(33.33, 0.01);
    }

    [Fact]
    public void ReturnZeroPercentagesWhenNoTests()
    {
        var summary = new TestResultsSummary();

        summary.PercentageOfPassedTests.Should().Be(0.0);
        summary.PercentageOfFailedTests.Should().Be(0.0);
        summary.PercentageOfSkippedTests.Should().Be(0.0);
    }

    [Fact]
    public void SumPassedAndFailedDurationsForTimeElapsed()
    {
        var summary = new TestResultsSummary();
        summary.AddPassedTest(new TestResult.Passed(100) { TestName = "p1", TestCasePath = "" });
        summary.AddPassedTest(new TestResult.Passed(200) { TestName = "p2", TestCasePath = "" });
        summary.AddFailedTest(new TestResult.Failed(50, "err", null) { TestName = "f1", TestCasePath = "" });

        summary.TimeElapsedDuringTesting.Should().Be(350);
    }
}
