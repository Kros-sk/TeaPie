using Microsoft.Extensions.Logging;
using TeaPie.Extensions;
using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines.TemporaryFolder;

internal sealed class PrepareTemporaryFolderStep(IPipeline pipeline) : IPipelineStep
{
    private readonly IPipeline _pipeline = pipeline;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        if (context.TempFolderPath.Equals(string.Empty))
        {
            context.TempFolderPath = Path.Combine(Path.GetTempPath(), Constants.ApplicationName);

            if (Directory.Exists(context.TempFolderPath))
            {
                await context.ServiceProvider.GetStep<CleanUpTemporaryFolderStep>().Execute(context, cancellationToken);
                context.Logger.LogTrace(
                    "Since temporary folder structure on path '{TempPath}' already existed, it was deleted to prevent " +
                    "incorrect structure of folders.",
                    context.TempFolderPath);
            }

            _pipeline.AddSteps(context.ServiceProvider.GetStep<CleanUpTemporaryFolderStep>());
            context.Logger.LogDebug("Temporary folder clean-up step was scheduled at the end of the pipeline.");
        }

        if (!Directory.Exists(context.TempFolderPath))
        {
            Directory.CreateDirectory(context.TempFolderPath);
            context.Logger.LogTrace("Temporary folder on path '{TempFolderPath}' was created.", context.TempFolderPath);
        }
    }
}
