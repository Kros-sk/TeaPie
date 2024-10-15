using Microsoft.Extensions.DependencyInjection;
using TeaPie.Pipelines.Application;
using TeaPie.StructureExploration;

namespace TeaPie.Pipelines.StructureExploration;

internal sealed class StructureExplorationStep : IPipelineStep
{
    private readonly IStructureExplorer _structureExplorer;

    private StructureExplorationStep(IStructureExplorer structureExplorer)
    {
        _structureExplorer = structureExplorer;
    }

    public static StructureExplorationStep Create(IServiceProvider serviceProvider)
        => new(serviceProvider.GetRequiredService<IStructureExplorer>());

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            context.TestCases = _structureExplorer.ExploreFileSystem(context.Path);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new Exception($"Structure exploration failed. Cause: {ex.Message}");
        }
    }
}
