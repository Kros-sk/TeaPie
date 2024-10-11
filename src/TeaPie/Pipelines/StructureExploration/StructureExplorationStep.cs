using Microsoft.Extensions.DependencyInjection;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Base;
using TeaPie.StructureExploration;

namespace TeaPie.Pipelines.StructureExploration;
internal class StructureExplorationStep : IPipelineStep
{
    private readonly IStructureExplorer _structureExplorer;

    private StructureExplorationStep(IStructureExplorer structureExplorer)
    {
        _structureExplorer = structureExplorer;
    }

    public static StructureExplorationStep Create(IServiceProvider serviceProvider)
        => new(serviceProvider.GetRequiredService<IStructureExplorer>());

    public async Task<ApplicationContext> ExecuteAsync(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            context.TestCases = _structureExplorer.ExploreFileSystem(context.Path);
            await Task.CompletedTask;
            return context;
        }
        catch (Exception ex)
        {
            throw new Exception($"Structure exploration failed. Cause: {ex.Message}");
        }
    }
}
