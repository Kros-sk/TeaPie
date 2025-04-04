using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using TeaPie.Pipelines;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.StructureExploration;

internal sealed class ResolvePathsStep(IPathProvider pathProvider) : IPipelineStep
{
    private readonly IPathProvider _pathProvider = pathProvider;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ResolvePaths(context);

        CreateTempFolderIfNeeded(context);

        _pathProvider.UpdatePaths(context.Path, context.TempFolderPath, context.TeaPieFolderPath);

        LogUpdatedPaths(context);

        await Task.CompletedTask;
    }

    private static void ResolvePaths(ApplicationContext context)
    {
        if (context.TempFolderPath.Equals(string.Empty))
        {
            if (TryFindTeaPieFolder(context.Path, out var teaPiePath))
            {
                context.TempFolderPath = teaPiePath;
                context.TeaPieFolderPath = teaPiePath;
            }
            else
            {
                context.TempFolderPath = Constants.SystemTemporaryFolderPath;
            }
        }
    }

    private static bool TryFindTeaPieFolder(string startingPoint, [NotNullWhen(true)] out string? teaPiePath)
    {
        teaPiePath = null;
        var current = new DirectoryInfo(startingPoint);

        while (current?.Parent is not null)
        {
            if (TryFindInCurrent(current, out teaPiePath) ||
                TryFindInSiblings(current, out teaPiePath))
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    private static bool TryFindInCurrent(DirectoryInfo directory, [NotNullWhen(true)] out string? teaPieFolder)
    {
        teaPieFolder = Path.Combine(directory.FullName, Constants.TeaPieFolderName);
        return Directory.Exists(teaPieFolder);
    }

    private static bool TryFindInSiblings(DirectoryInfo directory, [NotNullWhen(true)] out string? teaPieFolder)
    {
        teaPieFolder = Path.Combine(directory.Parent!.FullName, Constants.TeaPieFolderName);
        return Directory.Exists(teaPieFolder);
    }

    private static void CreateTempFolderIfNeeded(ApplicationContext context)
    {
        if (!Directory.Exists(context.TempFolderPath))
        {
            Directory.CreateDirectory(context.TempFolderPath);
            context.Logger.LogDebug(
                "Temporary folder was created at path '{TempPath}', since it didn't exist yet.", context.TempFolderPath);
        }
    }

    private void LogUpdatedPaths(ApplicationContext context)
        => context.Logger.LogDebug("Application is working with these paths:{NewLine}" +
            "Path: '{Path}'{NewLine}" +
            "Temporary folder path: '{TempPath}'{NewLine}" +
            "TeaPie folder path: '{TeaPieFolderPath}'",
            Environment.NewLine,
            _pathProvider.RootPath,
            Environment.NewLine,
            _pathProvider.TempRootPath,
            Environment.NewLine,
            _pathProvider.TeaPieFolderPath);
}
