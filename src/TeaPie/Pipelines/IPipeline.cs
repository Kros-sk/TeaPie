using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines;

internal interface IPipeline
{
    Task Run(ApplicationContext context, CancellationToken cancellationToken = default);

    void AddSteps(params IPipelineStep[] steps);

    void AddSteps(params Func<ApplicationContext, CancellationToken, Task>[] lambdaFunction);

    void InsertSteps(IPipelineStep predecessor, params Func<ApplicationContext, CancellationToken, Task>[] lambdaFunction);

    void InsertSteps(IPipelineStep predecessor, params IPipelineStep[] steps);
}
