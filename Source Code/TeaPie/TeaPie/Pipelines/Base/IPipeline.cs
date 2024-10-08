using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines.Base;

internal interface IPipeline
{
    internal Task<ApplicationContext> RunAsync(ApplicationContext context, CancellationToken cancellationToken = default);
    internal void AddStep(IPipelineStep step);
}
