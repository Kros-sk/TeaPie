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
        // If no temporary folder path is explicitly set, default system temporary folder path is used
        if (context.TempFolderPath.Equals(string.Empty))
        {
            context.TempFolderPath = Path.Combine(Path.GetTempPath(), Constants.ApplicationName);

            // If temporary folder already exists, it should be cleaned up before new set of folders will be placed in
            if (Directory.Exists(context.TempFolderPath))
            {
                await CleanUpTemporaryFolderStep.Create().Execute(context, cancellationToken);
            }

            // If default temporary path is used, clean up has to be done in the end of pipeline
            _pipeline.InsertStep(CleanUpTemporaryFolderStep.Create());
        }

        Directory.CreateDirectory(context.TempFolderPath);
    }
}
