using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using TeaPie.Pipelines;

namespace TeaPie.StructureExploration;

internal sealed class ResolveTemporaryFolderStep : IPipelineStep
{
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

        await Task.CompletedTask;
    }

    private static void ResolvePath(ApplicationContext context)
    {
        if (TryFindTeaPieFolder(context.Path, out var teaPiePath))
        {
            context.TempFolderPath = teaPiePath;
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
        while (!currentPath.Equals(Environment.SystemDirectory))
        {
            if (Path.GetFileName(currentPath).Equals(Constants.TeaPieFolderName))
            {
                teaPiePath = currentPath;
                return true;
            }

            currentPath = Directory.GetParent(currentPath)?.FullName
                ?? throw new InvalidOperationException($"Given path {currentPath} doesn't have any parent folder.");
        }

        return false;
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
