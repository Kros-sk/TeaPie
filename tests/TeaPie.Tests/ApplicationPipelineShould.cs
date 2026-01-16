using FluentAssertions;
using NSubstitute;
using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
using TeaPie.Testing;
using TeaPie.Tests.Pipelines;

namespace TeaPie.Tests;

public class ApplicationPipelineShould
{
    [Fact]
    public async Task ExecuteAllStepsDuringRun()
    {
        var pipeline = new ApplicationPipeline();
        var cancellationToken = CancellationToken.None;
        var context = CreateApplicationContext(string.Empty);
        var steps = new IPipelineStep[3];
        IPipelineStep step;

        for (var i = 0; i < steps.Length; i++)
        {
            step = Substitute.For<IPipelineStep>();

            step.ShouldExecute(context).Returns(true);
            pipeline.AddSteps(step);
            steps[i] = step;
        }

        await pipeline.Run(context);

        for (var i = 0; i < steps.Length; i++)
        {
            await steps[i].Received(1).Execute(context, cancellationToken);
        }
    }

    [Fact]
    public async Task ExecuteStepsInCorrectOrderWhenInsertingStepsOneByOne()
    {
        var pipeline = new ApplicationPipeline();
        var context = CreateApplicationContext(string.Empty);
        var steps = new IdentifyingStep[5];
        var registerOfSteps = new List<int>();

        for (var i = 0; i < steps.Length; i++)
        {
            steps[i] = new(registerOfSteps, i);
        }

        pipeline.AddSteps(steps[0]);
        pipeline.AddSteps(steps[2]);
        pipeline.InsertSteps(steps[0], steps[1]);
        pipeline.AddSteps(steps[4]);
        pipeline.InsertSteps(steps[2], steps[3]);

        await pipeline.Run(context);

        for (var i = 0; i < steps.Length; i++)
        {
            Assert.Equal(steps[i].Id, registerOfSteps[i]);
        }
    }

    [Fact]
    public async Task ExecuteStepsInCorrectOrderWhenInsertingStepsInRange()
    {
        var pipeline = new ApplicationPipeline();
        var context = CreateApplicationContext(string.Empty);
        var steps = new IdentifyingStep[7];
        var registerOfSteps = new List<int>();

        for (var i = 0; i < steps.Length; i++)
        {
            steps[i] = new(registerOfSteps, i);
        }

        pipeline.AddSteps(steps[0]);
        pipeline.AddSteps(steps[4]);
        pipeline.InsertSteps(steps[0], steps[1], steps[2], steps[3]);
        pipeline.InsertSteps(steps[4], steps[5], steps[6]);

        await pipeline.Run(context);

        for (var i = 0; i < steps.Length; i++)
        {
            Assert.Equal(steps[i].Id, registerOfSteps[i]);
        }
    }

    [Fact]
    public async Task EnableAddingStepsDuringPipelineRun()
    {
        var pipeline = new ApplicationPipeline();
        pipeline.AddSteps(new GenerativeStep(pipeline));

        var context = CreateApplicationContext(string.Empty);

        await pipeline.Run(context);
    }

    [Fact]
    public async Task ExecuteScheduledTestsBeforeRegisteredTests()
    {
        var pipeline = new ApplicationPipeline();
        var executionOrder = new List<string>();

        var testCase = new TestCase(new InternalFile("test.http", "test.http", null!));
        var testCaseContext = new TestCaseExecutionContext(testCase);

        var accessor = Substitute.For<ITestCaseExecutionContextAccessor>();
        accessor.Context.Returns(testCaseContext);

        var reporter = Substitute.For<ITestResultsSummaryReporter>();

        var tester = Substitute.For<ITester>();
        tester.ExecuteOrSkipTest(Arg.Any<Test>(), Arg.Any<TestCase?>())
            .Returns(callInfo =>
            {
                var test = callInfo.Arg<Test>();
                executionOrder.Add(test.Name);
                return Task.FromResult(test);
            });

        var scheduler = new TestScheduler();
        var scheduledTest = CreateTest("scheduled-test", testCase);
        scheduler.Schedule(scheduledTest);

        var registeredTest = CreateTest("registered-test", testCase);
        testCaseContext.RegisterTest(registeredTest);

        var executeScheduledTestsStep = new ExecuteScheduledTestsStep(scheduler, tester);
        var runScriptTestsStep = new RunScriptTestsStep(accessor, reporter, tester);

        pipeline.AddSteps(executeScheduledTestsStep);
        pipeline.AddSteps(runScriptTestsStep);

        await pipeline.Run(CreateApplicationContext(string.Empty));

        executionOrder.Should().HaveCount(2);
        executionOrder[0].Should().Be("scheduled-test");
        executionOrder[1].Should().Be("registered-test");
    }

    private static Test CreateTest(string name, TestCase testCase)
    {
        var result = new TestResult.NotRun { TestName = name, TestCasePath = "test.http" };
        return new Test(name, false, () => Task.CompletedTask, result, testCase);
    }

    private static ApplicationContext CreateApplicationContext(string path)
        => new ApplicationContextBuilder()
            .WithPath(path)
            .Build();
}
