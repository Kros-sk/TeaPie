using FluentAssertions;
using TeaPie.Testing;

namespace TeaPie.Tests.Testing;

public class TestCaseTestResultsSummaryShould
{
    [Fact]
    public void SetNameFromConstructor()
    {
        var summary = new TestCaseTestResultsSummary("MyTestCase");

        summary.Name.Should().Be("MyTestCase");
    }

    [Fact]
    public void AddPassedTestViaInheritedBehavior()
    {
        var summary = new TestCaseTestResultsSummary("tc");
        var passed = new TestResult.Passed(150) { TestName = "test1", TestCasePath = "" };

        summary.AddPassedTest(passed);

        summary.NumberOfPassedTests.Should().Be(1);
        summary.PassedTests.Should().ContainSingle().Which.Should().Be(passed);
    }
}
