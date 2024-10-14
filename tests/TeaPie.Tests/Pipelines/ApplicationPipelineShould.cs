using NSubstitute;
using TeaPie.Pipelines;
using TeaPie.Pipelines.Application;

namespace TeaPie.Tests.Pipelines;

public class ApplicationPipelineShould
{
    [Fact]
    public async void PipelineRunShouldExecuteAllSteps()
    {
        var pipeline = new ApplicationPipeline();
        var cancellationToken = CancellationToken.None;
        var context = new ApplicationContext(string.Empty);
        var steps = new IPipelineStep[3];
        IPipelineStep step;

        for (var i = 0; i < steps.Length; i++)
        {
            step = Substitute.For<IPipelineStep>();

            pipeline.InsertStep(step);
            steps[i] = step;
        }

        await pipeline.Run(context);

        for (var i = 0; i < steps.Length; i++)
        {
            await steps[i].Received(1).Execute(context, cancellationToken);
        }
    }

    [Fact]
    public async Task PipelineStepsShouldExecuteInCorrectOrder()
    {
        var pipeline = new ApplicationPipeline();
        var cancellationToken = CancellationToken.None;
        var context = new ApplicationContext(string.Empty);
        var steps = new IdentifyingStep[5];
        var registerOfSteps = new List<int>();

        for (var i = 0; i < steps.Length; i++)
        {
            steps[i] = new(registerOfSteps, i);
        }

        pipeline.InsertStep(steps[0]);
        pipeline.InsertStep(steps[2]);
        pipeline.InsertStep(steps[1], steps[0]);
        pipeline.InsertStep(steps[4]);
        pipeline.InsertStep(steps[3], steps[2]);

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
        pipeline.InsertStep(new GenerativeStep(pipeline));

        var context = new ApplicationContext(string.Empty);

        await pipeline.Run(context);
    }

    private class GenerativeStep(IPipeline pipeline) : IPipelineStep
    {
        private readonly IPipeline _pipeline = pipeline;

        public async Task Execute(
            ApplicationContext context,
            CancellationToken cancellationToken = default)
        {
            _pipeline.InsertStep(new DummyStep());
            _pipeline.InsertStep(new DummyStep());
            _pipeline.InsertStep(new DummyStep());

            await Task.CompletedTask;
        }
    }

    private class DummyStep : IPipelineStep
    {
        public async Task Execute(
            ApplicationContext context,
            CancellationToken cancellationToken = default)
            => await Task.CompletedTask;
    }

    private class IdentifyingStep(List<int> register, int id) : IPipelineStep
    {
        public int Id { get; } = id;
        private readonly List<int> _register = register;

        public async Task Execute(
            ApplicationContext context,
            CancellationToken cancellationToken = default)
        {
            _register.Add(Id);
            await Task.CompletedTask;
        }
    }
}
