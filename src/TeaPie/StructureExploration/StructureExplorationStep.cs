using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;

namespace TeaPie.StructureExploration;

internal sealed class StructureExplorationStep(IStructureExplorer structureExplorer) : IPipelineStep
{
    private readonly IStructureExplorer _structureExplorer = structureExplorer;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            context.TestCases = _structureExplorer.ExploreCollectionStructure(context.Path);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            context.Logger.LogError("Exploration of the collection failed, reason: {error}.", ex.Message);
            throw;
        }
    }
}
