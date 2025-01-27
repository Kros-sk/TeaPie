using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;

namespace TeaPie;

internal class ApplicationPipeline : IPipeline
{
    private readonly StepsCollection _pipelineSteps = [];
    private bool _errorOccured;
    private IPipelineStep? _currentStep;

    public async Task<int> Run(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        context.Logger.LogDebug("Application pipeline started. Number of planned steps: {Count}.", _pipelineSteps.Count);

        var enumerator = _pipelineSteps.GetEnumerator();

        return await Run(context, enumerator, cancellationToken);
    }

    private async Task<int> Run(
        ApplicationContext context,
        IEnumerator<IPipelineStep> enumerator,
        CancellationToken cancellationToken)
    {
        IPipelineStep step;
        while (enumerator.MoveNext())
        {
            step = enumerator.Current;
            _currentStep = step;

            await ExecuteStep(step, context, cancellationToken);

            if (_errorOccured)
            {
                context.Logger.LogError("Error occured during pipeline run - terminated with exit code 1.");
                return 1;
            }
        }

        context.Logger.LogDebug("Application pipeline finished successfully. Number of executed steps: {Count}.",
                _pipelineSteps.Count);

        return 0;
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

    public void InsertSteps(IPipelineStep? predecessor, params IPipelineStep[] steps)
        => InsertStepsWithValidation(predecessor, steps);

    public void InsertSteps(
        IPipelineStep? predecessor,
        params Func<ApplicationContext, CancellationToken, Task>[] lambdaFunctions)
        => InsertStepsWithValidation(predecessor, lambdaFunctions.Select(f => new InlineStep(f)));

    private IPipelineStep InsertStepsWithValidation(IPipelineStep? predecessor, IEnumerable<IPipelineStep> steps)
    {
        predecessor ??= _currentStep;

        if (predecessor is null)
        {
            throw new InvalidOperationException("Unable to insert steps to pipeline, if predecessor step is null. " +
                "This may occur, when currently executed step is not set to an instance of object.");
        }

        _pipelineSteps.InsertRange(predecessor, steps);
        return predecessor;
    }
}
