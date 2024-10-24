using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines;

internal class InlineStep : IPipelineStep
{
    private readonly Func<ApplicationContext, CancellationToken, Task> _lambdaFunction;

    internal InlineStep(Func<ApplicationContext, CancellationToken, Task> lambdaFunction)
    {
        _lambdaFunction = lambdaFunction;
    }

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
        => await _lambdaFunction.Invoke(context, cancellationToken);
}
