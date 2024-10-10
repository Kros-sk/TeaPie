using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines.Base;
internal class BasePipeline : IPipeline
{
    protected readonly List<IPipelineStep> _pipelineSteps = [];

    public virtual async Task<ApplicationContext> RunAsync(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        var enumerator = new PipelineStepsEnumerator(_pipelineSteps);

        IPipelineStep step;
        ApplicationContext input, result = context;
        while (enumerator.MoveNext())
        {
            step = enumerator.Current;
            input = result;
            result = await step.ExecuteAsync(input, cancellationToken);
        }

        return result;
    }

    public void AddStep(IPipelineStep step) => _pipelineSteps.Add(step);
}
