using TeaPie.Pipelines.Base;

namespace TeaPie.Pipelines.Application;
internal class ApplicationPipeline : IPipeline
{
    private readonly List<IPipelineStep> _pipelineSteps = [];

    public async Task<ApplicationContext> RunAsync(ApplicationContext context, CancellationToken cancellationToken)
    {
        var enumerator = new ApplicationPipelineEnumerator(_pipelineSteps);

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
