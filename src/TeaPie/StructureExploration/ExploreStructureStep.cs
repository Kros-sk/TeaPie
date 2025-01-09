using TeaPie.Pipelines;

namespace TeaPie.StructureExploration;

internal sealed class ExploreStructureStep(IStructureExplorer structureExplorer) : IPipelineStep
{
    private readonly IStructureExplorer _structureExplorer = structureExplorer;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        context.CollectionStructure = _structureExplorer.ExploreCollectionStructure(context.Path);
        await Task.CompletedTask;
    }
}
