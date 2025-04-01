using Microsoft.Extensions.Logging;
using Spectre.Console;
using TeaPie.Pipelines;

namespace TeaPie;

internal class ApplicationPipeline : IPipeline
{
    private readonly StepsCollection _pipelineSteps = [];
    private IPipelineStep? _currentStep;

    public async Task<int> Run(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        var enumerator = _pipelineSteps.GetEnumerator();
        return await Run(context, enumerator, cancellationToken);
    }

    private async Task<int> Run(
        ApplicationContext context,
        IEnumerator<IPipelineStep> enumerator,
        CancellationToken cancellationToken)
    {
        LogStartOfRun(context);

        IPipelineStep step;
        while (enumerator.MoveNext())
        {
            step = enumerator.Current;

            if (step.ShouldExecute(context))
            {
                _currentStep = step;

                await ExecuteStep(step, context, cancellationToken);
            }
        }

        LogEndOfRun(context);

        return ComputeExitCode(context);
    }

    private void LogStartOfRun(ApplicationContext context)
        => context.Logger.LogDebug("Application pipeline started. Number of planned steps: {Count}.", _pipelineSteps.Count);

    private static int ComputeExitCode(ApplicationContext context)
        => context.PrematureTermination is null
            ? 0
            : (int)context.PrematureTermination.Type;

    private void LogEndOfRun(ApplicationContext context)
    {
        if (context.PrematureTermination is not null)
        {
            LogPrematureTerminationIfNeeded(context);
        }
        else
        {
            context.Logger.LogDebug("Application pipeline finished successfully. Number of executed steps: {Count}.",
                _pipelineSteps.Count);
        }
    }

    private static void LogPrematureTerminationIfNeeded(ApplicationContext context)
    {
        var termination = context.PrematureTermination!;
        context.Logger.LogError("'{Source}' caused premature termination of application run (exit code 1)." +
            "{NewLine}Reason: {Reason}{NewLine}Details: {Details}",
            termination.Source,
            Environment.NewLine,
            Enum.GetName(termination.Type)?.SplitPascalCase(),
            Environment.NewLine,
            termination.Details);
    }

    private static async Task ExecuteStep(IPipelineStep step, ApplicationContext context, CancellationToken cancellationToken)
    {
        try
        {
            await step.Execute(context, cancellationToken);
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(context, ex, step.GetType(), context.Logger);
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
