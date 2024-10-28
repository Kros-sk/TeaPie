using Microsoft.Extensions.Logging;
using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines.TemporaryFolder;

internal sealed class CleanUpTemporaryFolderStep : IPipelineStep
{
    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(context.TempFolderPath))
        {
            throw new DirectoryNotFoundException($"Directory on path {context.TempFolderPath} does not exist.");
        }

        Directory.Delete(context.TempFolderPath, true);
        await Task.CompletedTask;

        context.Logger.LogTrace("Temporary folder on path '{TempFolderPath}' was deleted.", context.TempFolderPath);
    }
}
