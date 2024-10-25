using Microsoft.Extensions.Logging;
using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines.TemporaryFolder;

internal sealed class CleanUpTemporaryFolderStep : IPipelineStep
{
    private CleanUpTemporaryFolderStep() { }

    public static CleanUpTemporaryFolderStep Create() => new();

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        Directory.Delete(context.TempFolderPath, true);
        await Task.CompletedTask;

        context.Logger.LogTrace("Temporary folder on path '{TempFolderPath}' was deleted.", context.TempFolderPath);
    }
}
