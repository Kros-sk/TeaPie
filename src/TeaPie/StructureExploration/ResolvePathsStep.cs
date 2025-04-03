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
        if (context.TempFolderPath.Equals(string.Empty))
        {
            ResolvePath(context);
        }

        CreateFolderIfNeeded(context);

        _pathProvider.UpdatePaths(context.Path, context.TempFolderPath, context.TeaPieFolderPath);
        await Task.CompletedTask;
    }

    private static void ResolvePath(ApplicationContext context)
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

    private static void CreateFolderIfNeeded(ApplicationContext context)
    {
        if (!Directory.Exists(context.TempFolderPath))
        {
            Directory.CreateDirectory(context.TempFolderPath);
            context.Logger.LogDebug(
                "Temporary folder was created on path '{TempPath}', since it didn't exist yet.", context.TempFolderPath);
        }
    }
}
