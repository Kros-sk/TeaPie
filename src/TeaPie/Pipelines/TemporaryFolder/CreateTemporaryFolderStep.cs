using TeaPie.Pipelines.Application;

namespace TeaPie.Pipelines.TemporaryFolder;

internal sealed class CreateTemporaryFolderStep : IPipelineStep
{
    private CreateTemporaryFolderStep() { }

    public static CreateTemporaryFolderStep Create() => new();

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ResolveTempPath(context);
        Directory.CreateDirectory(context.TempFolderPath);
        await Task.CompletedTask;
    }

    private static void ResolveTempPath(ApplicationContext context)
    {
        if (context.TempFolderPath.Equals(string.Empty))
        {
            context.TempFolderPath = Path.Combine(Path.GetTempPath(), Constants.ApplicationName);
        }
    }
}
