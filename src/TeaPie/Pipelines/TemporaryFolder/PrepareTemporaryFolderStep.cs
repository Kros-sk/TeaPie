using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines.TemporaryFolder;

internal sealed class PrepareTemporaryFolderStep : IPipelineStep
{
    private readonly IPipeline _pipeline;

    private PrepareTemporaryFolderStep(IPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public static PrepareTemporaryFolderStep Create(IPipeline pipeline) => new(pipeline);

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        if (context.TempFolderPath.Equals(string.Empty))
        {
            context.TempFolderPath = Path.Combine(Path.GetTempPath(), Constants.ApplicationName);

            if (Directory.Exists(context.TempFolderPath))
            {
                await CleanUpTemporaryFolderStep.Create().Execute(context, cancellationToken);
            }

            _pipeline.AddSteps(CleanUpTemporaryFolderStep.Create());
        }

        Directory.CreateDirectory(context.TempFolderPath);
    }
}
