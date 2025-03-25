using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using TeaPie.Pipelines;

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
        else
        {
            CreateFolderIfNeeded(context);
        }

        _pathProvider.UpdatePaths(context);
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
        var currentPath = startingPoint;

        while (Directory.GetParent(currentPath) is not null)
        {
            if (IsTeaPieFolder(currentPath))
            {
                teaPiePath = currentPath;
                return true;
            }

            if (TryFindFromSiblings(currentPath, out teaPiePath))
            {
                return true;
            }

            currentPath = Directory.GetParent(currentPath)?.FullName!;
        }

        return false;
    }

    private static bool TryFindFromSiblings(string currentPath, [NotNullWhen(true)] out string? teaPieFolder)
    {
        var parent = Directory.GetParent(currentPath)!;
        var siblings = Directory.GetDirectories(parent.FullName);
        teaPieFolder = siblings.FirstOrDefault(IsTeaPieFolder);

        return teaPieFolder is not null;
    }

    private static bool IsTeaPieFolder(string currentPath) => Path.GetFileName(currentPath).Equals(Constants.TeaPieFolderName);

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
