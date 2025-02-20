using Microsoft.Extensions.Logging;
using TeaPie.Application;
using TeaPie.Pipelines;

namespace TeaPie.StructureExploration;

internal sealed class PrepareTemporaryFolderStep(IPipeline pipeline) : IPipelineStep
{
    private readonly IPipeline _pipeline = pipeline;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        if (context.TempFolderPath.Equals(Constants.DefaultTemporaryFolderPath))
        {
            await CleanTemporaryFolderIfExists(context, cancellationToken);

            _pipeline.AddSteps(context.ServiceProvider.GetStep<CleanUpTemporaryFolderStep>());
            context.Logger.LogDebug("Temporary folder clean-up step was scheduled at the end of the pipeline.");
        }

        CreateTemporaryFolderIfDoesntExistYet(context);
    }

    private static async Task CleanTemporaryFolderIfExists(ApplicationContext context, CancellationToken cancellationToken)
    {
        if (Directory.Exists(context.TempFolderPath))
        {
            await context.ServiceProvider.GetStep<CleanUpTemporaryFolderStep>().Execute(context, cancellationToken);
            context.Logger.LogTrace(
                "Since temporary folder structure on path '{TempPath}' already existed, it was deleted to prevent " +
                "incorrect structure of folders.",
                context.TempFolderPath);
        }
    }

    private static void CreateTemporaryFolderIfDoesntExistYet(ApplicationContext context)
    {
        if (!Directory.Exists(context.TempFolderPath))
        {
            Directory.CreateDirectory(context.TempFolderPath);
            context.Logger.LogTrace("Temporary folder on path '{TempFolderPath}' was created.", context.TempFolderPath);
        }
    }
}
