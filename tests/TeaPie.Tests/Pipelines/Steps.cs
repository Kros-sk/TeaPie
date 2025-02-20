using TeaPie.Application;
using TeaPie.Pipelines;

namespace TeaPie.Tests.Pipelines;

internal class DummyStep : IPipelineStep
{
    public async Task Execute(
        ApplicationContext context,
        CancellationToken cancellationToken = default)
        => await Task.CompletedTask;
}

internal class IdentifyingStep(List<int> register, int id) : IPipelineStep
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

internal class GenerativeStep(IPipeline pipeline) : IPipelineStep
{
    private readonly IPipeline _pipeline = pipeline;

    public async Task Execute(
        ApplicationContext context,
        CancellationToken cancellationToken = default)
    {
        _pipeline.AddSteps(new DummyStep());
        _pipeline.AddSteps(new DummyStep());
        _pipeline.AddSteps(new DummyStep());

        await Task.CompletedTask;
    }
}
