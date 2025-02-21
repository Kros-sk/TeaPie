namespace TeaPie.Pipelines;

internal interface IPipeline
{
    Task<int> Run(ApplicationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds <paramref name="steps"/> to the end of the pipeline.
    /// </summary>
    /// <param name="steps">Steps to be added to the end of the pipeline.</param>
    void AddSteps(params IPipelineStep[] steps);

    /// <summary>
    /// Adds steps defined by <paramref name="lambdaFunctions"/> to the end of the pipeline.
    /// </summary>
    /// <param name="lambdaFunctions">Lambda functions representing steps to be added to the end of the pipeline.</param>
    void AddSteps(params Func<ApplicationContext, CancellationToken, Task>[] lambdaFunctions);

    /// <summary>
    /// Inserts steps defined by <paramref name="lambdaFunctions"/> after <paramref name="predecessor"/>.
    /// If <paramref name="predecessor"/> is <see langword="null"/>, steps are added after the currently executed step.
    /// </summary>
    /// <param name="predecessor">The step after which the new steps will be inserted, or <see langword="null"/> to
    /// insert after the currently executed step.</param>
    /// <param name="lambdaFunctions">Lambda functions representing steps to insert.</param>
    void InsertSteps(IPipelineStep? predecessor, params Func<ApplicationContext, CancellationToken, Task>[] lambdaFunctions);

    /// <summary>
    /// Inserts <paramref name="steps"/> after <paramref name="predecessor"/>.
    /// If <paramref name="predecessor"/> is <see langword="null"/>, steps are added after the currently executed step.
    /// </summary>
    /// <param name="predecessor">The step after which the new steps will be inserted, or <see langword="null"/> to
    /// insert after the current step.</param>
    /// <param name="steps">Steps to insert after <paramref name="predecessor"/>.</param>
    void InsertSteps(IPipelineStep? predecessor, params IPipelineStep[] steps);
}
