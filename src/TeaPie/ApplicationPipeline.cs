using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;

namespace TeaPie;

internal class ApplicationPipeline : IPipeline
{
    protected readonly StepsCollection _pipelineSteps = [];

    public async Task Run(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        context.Logger.LogDebug("Application pipeline started. Number of planned steps: {Count}.", _pipelineSteps.Count);

        var enumerator = _pipelineSteps.GetEnumerator();

        IPipelineStep step;
        while (enumerator.MoveNext())
        {
            step = enumerator.Current;
            await step.Execute(context, cancellationToken);
        }

        context.Logger.LogDebug("Application pipeline finished successfully. Number of executed steps: {Count}.",
            _pipelineSteps.Count);
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
