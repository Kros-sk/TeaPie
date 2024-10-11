using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Base;
using TeaPie.StructureExploration;

namespace TeaPie.Pipelines.StructureExploration;
internal class StructureExplorationStep : IPipelineStep
{
    public async Task<ApplicationContext> ExecuteAsync(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            context.TestCases = StructureExplorer.ExploreFileSystem(context.Path);
            await Task.CompletedTask;
            return context;
        }
        catch (Exception ex)
        {
            throw new Exception($"Structure exploration failed. Cause: {ex.Message}");
        }
    }
}
