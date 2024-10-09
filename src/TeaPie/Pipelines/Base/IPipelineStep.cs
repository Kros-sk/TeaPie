using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines.Base;
internal interface IPipelineStep
{
    public Task<ApplicationContext> ExecuteAsync(ApplicationContext context, CancellationToken cancellationToken = default);
}
