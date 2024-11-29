namespace TeaPie.Pipelines;

internal class InlineStep(Func<ApplicationContext, CancellationToken, Task> lambdaFunction) : IPipelineStep
{
    private readonly Func<ApplicationContext, CancellationToken, Task> _lambdaFunction = lambdaFunction;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
        => await _lambdaFunction.Invoke(context, cancellationToken);
}
