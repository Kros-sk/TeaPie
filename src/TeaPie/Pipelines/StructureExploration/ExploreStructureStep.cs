using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Base;

namespace TeaPie.Pipelines.StructureExploration;
internal class ExploreStructureStep : IPipelineStep
{
    private readonly ExploreStructurePipeline _pipeline = ExploreStructurePipeline.CreateDefault();
    public async Task<ApplicationContext> ExecuteAsync(ApplicationContext context, CancellationToken cancellationToken = default)
        => await _pipeline.RunAsync(context, cancellationToken);
}
