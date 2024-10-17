using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines;

internal interface IPipeline
{
    public Task Run(ApplicationContext context, CancellationToken cancellationToken = default);
    public bool InsertStep(IPipelineStep step, IPipelineStep? predecessor = null);
    public bool InsertStep(Func<ApplicationContext, Task> lambdaFunction, IPipelineStep? predecessor = null);
    public bool InsertSteps(IEnumerable<IPipelineStep> steps, IPipelineStep? predecessor = null);
}
