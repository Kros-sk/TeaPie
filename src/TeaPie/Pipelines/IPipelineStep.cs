using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines;

internal interface IPipelineStep
{
    public Task Execute(ApplicationContext context, CancellationToken cancellationToken = default);
}
