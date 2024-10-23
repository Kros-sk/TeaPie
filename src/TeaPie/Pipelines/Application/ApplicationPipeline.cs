
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
            step = enumerator.Current;
            await step.Execute(context, cancellationToken);
        }
    }

    public bool InsertStep(IPipelineStep step, IPipelineStep? predecessor = null)
        => _pipelineSteps.Insert(step, predecessor);

    public bool InsertSteps(IEnumerable<IPipelineStep> steps, IPipelineStep? predecessor = null)
        => _pipelineSteps.InsertRange(steps, predecessor);

    public bool InsertStep(Func<ApplicationContext, Task> lambdaFunction, IPipelineStep? predecessor = null)
        => _pipelineSteps.Insert(new InlineStep(lambdaFunction), predecessor);
}
