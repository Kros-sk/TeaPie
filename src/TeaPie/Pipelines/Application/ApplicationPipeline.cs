
namespace TeaPie.Pipelines.Application;

internal class ApplicationPipeline : IPipeline
{
    protected readonly StepsCollection _pipelineSteps = [];

    public async Task Run(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        var enumerator = _pipelineSteps.GetEnumerator();

        IPipelineStep step;
        while (enumerator.MoveNext())
        {
            step = enumerator.Current;
            await step.Execute(context, cancellationToken);
        }
    }

    public void AddSteps(params IPipelineStep[] steps)
        => _pipelineSteps.AddRange(steps);

    public void AddSteps(params Func<ApplicationContext, CancellationToken, Task>[] lambdaFunctions)
        => _pipelineSteps.AddRange(lambdaFunctions.Select(function => new InlineStep(function)));

    public void InsertSteps(IPipelineStep predecessor, params IPipelineStep[] steps)
        => _pipelineSteps.InsertRange(predecessor, steps);

    public void InsertSteps(
        IPipelineStep predecessor,
        params Func<ApplicationContext, CancellationToken, Task>[] lambdaFunctions)
        => _pipelineSteps.InsertRange(predecessor, lambdaFunctions.Select(function => new InlineStep(function)));
}
