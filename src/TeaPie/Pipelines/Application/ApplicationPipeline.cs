namespace TeaPie.Pipelines.Application;

internal class ApplicationPipeline : IPipeline
{
    protected readonly StepsCollection _pipelineSteps = new();

    public async Task Run(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        var enumerator = _pipelineSteps.GetEnumerator();

        IPipelineStep step;
        while (enumerator.MoveNext())
        {
            step = enumerator.Current!; // Current can not be null, if MoveNext() was successfull
            await step.Execute(context, cancellationToken);
        }
    }

    /// <summary>
    /// Insert step just right after <param cref="predecessor">. If no predecessor is passed, step is
    /// added to the end of colllection.
    /// </summary>
    /// <param name="step">Pipeline step to be added.</param>
    /// <param name="predecessor">Predecessor of the step. If null, last element is cosidered.</param>
    /// <returns>Returns wheter step was successfully inserted.</returns>
    public bool InsertStep(IPipelineStep step, IPipelineStep? predecessor = null)
        => _pipelineSteps.Insert(step, predecessor);
}
