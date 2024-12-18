using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;

namespace TeaPie;

internal class ApplicationPipeline : IPipeline
{
    private readonly StepsCollection _pipelineSteps = [];
    private bool _errorOccured;

    public async Task Run(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        context.Logger.LogDebug("Application pipeline started. Number of planned steps: {Count}.", _pipelineSteps.Count);

        var enumerator = _pipelineSteps.GetEnumerator();

        await Run(context, enumerator, cancellationToken);

        context.Logger.LogDebug("Application pipeline finished successfully. Number of executed steps: {Count}.",
            _pipelineSteps.Count);
    }

    private async Task Run(
        ApplicationContext context, IEnumerator<IPipelineStep> enumerator, CancellationToken cancellationToken)
    {
        IPipelineStep step;
        while (enumerator.MoveNext())
        {
            step = enumerator.Current;
            await ExecuteStep(step, context, cancellationToken);

            if (_errorOccured)
            {
                context.Logger.LogError("Error occured during pipeline run. Shutting down the application.");
                Environment.Exit(1);
            }
        }
    }

    private async Task ExecuteStep(IPipelineStep step, ApplicationContext context, CancellationToken cancellationToken)
    {
        try
        {
            await step.Execute(context, cancellationToken);
        }
        catch (Exception ex)
        {
            _errorOccured = true;
            ExceptionHandler.Handle(ex, step.GetType(), context.Logger);
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
