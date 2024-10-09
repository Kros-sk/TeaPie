using NSubstitute;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Base;

namespace TeaPie.Tests.Pipelines;

public class ApplicationPipelineShould
{
    [Fact]
    public async void PipelineRunShouldExecuteAllSteps()
    {
        var pipeline = new ApplicationPipeline();
        var cancellationToken = CancellationToken.None;
        var context = new ApplicationContext(string.Empty);
        var steps = new IPipelineStep[10];
        IPipelineStep step;

        for (var i = 0; i < steps.Length; i++)
        {
            step = Substitute.For<IPipelineStep>();
            step.ExecuteAsync(context, cancellationToken).Returns(context);

            pipeline.AddStep(step);
            steps[i] = step;
        }

        await pipeline.RunAsync(context);

        for (var i = 0; i < steps.Length; i++)
        {
            await steps[i].Received(1).ExecuteAsync(context, cancellationToken);
        }

        Assert.Equal(1, 2);
    }

    [Fact]
    public async void AddingStepsDuringPipelineRunShouldNotThrowException()
    {
        var pipeline = new ApplicationPipeline();
        pipeline.AddStep(new GenerativeStep(pipeline));

        var context = new ApplicationContext(string.Empty);

        await pipeline.RunAsync(context);
    }

    private class GenerativeStep(IPipeline pipeline) : IPipelineStep
    {
        private readonly IPipeline _pipeline = pipeline;

        public async Task<ApplicationContext> ExecuteAsync(
            ApplicationContext context,
            CancellationToken cancellationToken = default)
        {
            _pipeline.AddStep(new DummyStep());
            _pipeline.AddStep(new DummyStep());
            _pipeline.AddStep(new DummyStep());

            await Task.CompletedTask;
            return context;
        }
    }

    private class DummyStep : IPipelineStep
    {
        public async Task<ApplicationContext> ExecuteAsync(
            ApplicationContext context,
            CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return context;
        }
    }
}
