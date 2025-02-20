using Microsoft.Extensions.Logging;
using TeaPie.Application;
using TeaPie.Pipelines;

namespace TeaPie.StructureExploration;

internal sealed class CleanUpTemporaryFolderStep : IPipelineStep
{
    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        CheckExistenceOfTemporaryFolder(context);

        Directory.Delete(context.TempFolderPath, true);
        await Task.CompletedTask;

        context.Logger.LogTrace("Temporary folder on path '{TempFolderPath}' was deleted.", context.TempFolderPath);
    }

    private static void CheckExistenceOfTemporaryFolder(ApplicationContext context)
    {
        if (!Directory.Exists(context.TempFolderPath))
        {
            throw new InvalidOperationException("Unable to clean-up temporary directory, when directory " +
                $"on path {context.TempFolderPath} does not exist.");
        }
    }
}
