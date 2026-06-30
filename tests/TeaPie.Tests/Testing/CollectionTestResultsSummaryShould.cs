using FluentAssertions;
using TeaPie.Testing;

namespace TeaPie.Tests.Testing;

public class CollectionTestResultsSummaryShould
{
    [Fact]
    public void SetNameFromConstructor()
    {
        var summary = new CollectionTestResultsSummary("MyCollection");

        summary.Name.Should().Be("MyCollection");
    }

    [Fact]
    public void CreateTestCaseEntryAndAddPassedResult()
    {
        var summary = new CollectionTestResultsSummary("col");
        var passed = new TestResult.Passed(100) { TestName = "test1", TestCasePath = "" };

        summary.AddPassedTest("TestCaseA", passed);

        summary.TestCases.Should().ContainKey("TestCaseA");
        summary.TestCases["TestCaseA"].PassedTests.Should().ContainSingle().Which.Should().Be(passed);
        summary.NumberOfPassedTests.Should().Be(1);
    }

    [Fact]
    public void CreateTestCaseEntryAndAddFailedResult()
    {
        var summary = new CollectionTestResultsSummary("col");
        var failed = new TestResult.Failed(200, "error", null) { TestName = "test2", TestCasePath = "" };

        summary.AddFailedTest("TestCaseB", failed);

        summary.TestCases.Should().ContainKey("TestCaseB");
        summary.TestCases["TestCaseB"].FailedTests.Should().ContainSingle().Which.Should().Be(failed);
        summary.NumberOfFailedTests.Should().Be(1);
    }

    [Fact]
    public void CreateTestCaseEntryAndAddSkippedResult()
    {
        var summary = new CollectionTestResultsSummary("col");
        var skipped = new TestResult.NotRun() { TestName = "test3", TestCasePath = "" };

        summary.AddSkippedTest("TestCaseC", skipped);

        summary.TestCases.Should().ContainKey("TestCaseC");
        summary.TestCases["TestCaseC"].SkippedTests.Should().ContainSingle().Which.Should().Be(skipped);
        summary.NumberOfSkippedTests.Should().Be(1);
    }

    [Fact]
    public void AddMultipleResultsToSameTestCase()
    {
        var summary = new CollectionTestResultsSummary("col");
        var passed1 = new TestResult.Passed(100) { TestName = "t1", TestCasePath = "" };
        var passed2 = new TestResult.Passed(200) { TestName = "t2", TestCasePath = "" };

        summary.AddPassedTest("SameCase", passed1);
        summary.AddPassedTest("SameCase", passed2);

        summary.TestCases.Should().HaveCount(1);
        summary.TestCases["SameCase"].PassedTests.Should().HaveCount(2);
        summary.NumberOfPassedTests.Should().Be(2);
    }

    [Fact]
    public void ContainExpectedTestCaseEntries()
    {
        var summary = new CollectionTestResultsSummary("col");
        summary.AddPassedTest("CaseA", new TestResult.Passed(10) { TestName = "t1", TestCasePath = "" });
        summary.AddFailedTest("CaseB", new TestResult.Failed(20, "err", null) { TestName = "t2", TestCasePath = "" });
        summary.AddSkippedTest("CaseC", new TestResult.NotRun() { TestName = "t3", TestCasePath = "" });

        summary.TestCases.Should().HaveCount(3);
        summary.TestCases.Keys.Should().Contain(["CaseA", "CaseB", "CaseC"]);
    }
}
