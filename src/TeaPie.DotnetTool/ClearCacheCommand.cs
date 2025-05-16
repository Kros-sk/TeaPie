using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace TeaPie.DotnetTool;

internal class CacheSettings : CommandSettings
{
    [CommandOption("-g|--glob|--global")]
    [DefaultValue(false)]
    [Description("Indicates whether to clear global cache (in system temporary folder).")]
    public bool Global { get; init; }
}

internal class ClearCacheCommand : Command<CacheSettings>
{
    private const string CacheFolderName = "cache";

    public override int Execute(CommandContext context, CacheSettings settings)
    {
        TryDeleteCacheInTeaPieFolder();
        DeleteGlobalCacheIfNeeded(settings.Global);
        return 0;
    }

    private static void TryDeleteCacheInTeaPieFolder()
    {
        if (TryFindTeaPieFolder(Directory.GetCurrentDirectory(), out var teaPieFolderPath))
        {
            DeleteCacheFolderIfExists(teaPieFolderPath, ".teapie folder");
        }
        else
        {
            AnsiConsole.MarkupLine("[orange1].teapie folder was not found.[/]");
        }
    }

    private static void DeleteGlobalCacheIfNeeded(bool shouldDelete)
    {
        if (shouldDelete)
        {
            DeleteCacheFolderIfExists(Constants.SystemTemporaryFolderPath, "global cache folder");
        }
    }

    private static void DeleteCacheFolderIfExists(string rootFolderPath, string cacheLocation)
    {
        var cacheFolder = Path.Combine(rootFolderPath, CacheFolderName);
        if (Directory.Exists(cacheFolder))
        {
            Directory.Delete(cacheFolder, true);
            AnsiConsole.MarkupLine($"[green]Cache within {cacheLocation} was cleared.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[green]Nothing to clear within {cacheLocation}.[/]");
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
        => TryFindInCurrent(directory.FullName, out teaPieFolder);

    private static bool TryFindInSiblings(DirectoryInfo directory, [NotNullWhen(true)] out string? teaPieFolder)
        => TryFindInCurrent(directory.Parent!.FullName, out teaPieFolder);

    private static bool TryFindInCurrent(string basePath, [NotNullWhen(true)] out string? teaPieFolder)
    {
        teaPieFolder = Path.Combine(basePath, Constants.TeaPieFolderName);
        return Directory.Exists(teaPieFolder);
    }
}
