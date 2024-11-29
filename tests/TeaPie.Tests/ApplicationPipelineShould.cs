using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Pipelines;
using TeaPie.Tests.Pipelines;

namespace TeaPie.Tests;

public class ApplicationPipelineShould
{
    [Fact]
    public async void PipelineRunShouldExecuteAllSteps()
    {
        var pipeline = new ApplicationPipeline();
        var cancellationToken = CancellationToken.None;
        var context = CreateApplicationContext(string.Empty);
        var steps = new IPipelineStep[3];
        IPipelineStep step;

        for (var i = 0; i < steps.Length; i++)
        {
            step = Substitute.For<IPipelineStep>();

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
    public async Task PipelineStepsShouldBeExecutedInCorrectOrderWhenInsertedOneByOne()
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
    public async Task PipelineStepsShouldBeExecutedInCorrectOrderWhenInsertedAsRange()
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
    public async void AddingStepsDuringPipelineRunShouldNotThrowException()
    {
        var pipeline = new ApplicationPipeline();
        pipeline.AddSteps(new GenerativeStep(pipeline));

        var context = CreateApplicationContext(string.Empty);

        await pipeline.Run(context);
    }

    private static ApplicationContext CreateApplicationContext(string path)
        => new(path, Substitute.For<ILogger>(), Substitute.For<IServiceProvider>());
}
