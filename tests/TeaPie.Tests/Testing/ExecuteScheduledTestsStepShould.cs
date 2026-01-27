using NSubstitute;
using TeaPie.Reporting;
using TeaPie.StructureExploration;
using TeaPie.Testing;
using static Xunit.Assert;

namespace TeaPie.Tests.Testing;

public class ExecuteScheduledTestsStepShould
{
    [Fact]
    public async Task InitializeReporterBeforeExecutingTests()
    {
        var scheduler = new TestScheduler();
        var tester = Substitute.For<ITester>();
        var reporter = Substitute.For<ITestResultsSummaryReporter>();

        var testCase = new TestCase(new InternalFile("test.http", "test.http", null!));
        var test = new Test(
            "test-name",
            false,
            async () => await Task.CompletedTask,
            new TestResult.NotRun() { TestName = "test-name" },
            testCase);

        scheduler.Schedule(test);

        var step = new ExecuteScheduledTestsStep(scheduler, tester, reporter);
        var context = new ApplicationContextBuilder()
            .WithPath(string.Empty)
            .Build();

        await step.Execute(context);

        reporter.Received(1).Initialize();
    }

    [Fact]
    public async Task ExecuteAllScheduledTests()
    {
        var scheduler = new TestScheduler();
        var tester = Substitute.For<ITester>();
        var reporter = Substitute.For<ITestResultsSummaryReporter>();

        var testCase = new TestCase(new InternalFile("test.http", "test.http", null!));
        var test1 = CreateTest("test-1", testCase);
        var test2 = CreateTest("test-2", testCase);
        var test3 = CreateTest("test-3", testCase);

        scheduler.Schedule(test1);
        scheduler.Schedule(test2);
        scheduler.Schedule(test3);

        var step = new ExecuteScheduledTestsStep(scheduler, tester, reporter);
        var context = new ApplicationContextBuilder()
            .WithPath(string.Empty)
            .Build();

        await step.Execute(context);

        await tester.Received(1).ExecuteOrSkipTest(test1, test1.TestCase);
        await tester.Received(1).ExecuteOrSkipTest(test2, test2.TestCase);
        await tester.Received(1).ExecuteOrSkipTest(test3, test3.TestCase);

        False(scheduler.HasScheduledTest());
    }

    private static Test CreateTest(string name, TestCase testCase)
    {
        var result = new TestResult.NotRun { TestName = name, TestCasePath = "test.http" };
        return new Test(name, false, () => Task.CompletedTask, result, testCase);
    }
}
