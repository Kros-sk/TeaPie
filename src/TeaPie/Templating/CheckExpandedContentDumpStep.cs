using TeaPie.Pipelines;

namespace TeaPie.Templating;

internal sealed class CheckExpandedContentDumpStep(IExpandedContentDumper expandedContentDumper) : IPipelineStep
{
    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        expandedContentDumper.EnsureDumpsAreOutsideOfTestedCollection(context);
        await Task.CompletedTask;
    }
}
