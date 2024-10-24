using TeaPie.Pipelines;
using TeaPie.Pipelines.Application;

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
