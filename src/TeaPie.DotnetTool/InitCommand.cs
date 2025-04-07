using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace TeaPie.DotnetTool;

internal sealed class InitCommand : Command<InitCommand.Settings>
{
    private const string SearchingClue = ".git";
    private const string GitIgnoreFileName = ".gitignore";
    private readonly string[] _linesForGitignore =
    [
        $"{Environment.NewLine}# TeaPie",
        "**/.teapie/cache/",
        "**/.teapie/reports/"
    ];

    public override int Execute(CommandContext context, Settings settings)
    {
        var path = ResolvePath(settings);

        UpdateGitignore(path);
        CreateTeaPieFolder(path);

        AnsiConsole.MarkupLine("[green]Initialization of TeaPie was successful.[/]");
        return 0;
    }

    private static string ResolvePath(Settings settings)
    {
        if (!string.IsNullOrEmpty(settings.Path))
        {
            CreateIfNeeded(settings.Path);
            return settings.Path;
        }
        else
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var repositoryRoot = FindRepositoryRoot(currentDirectory);
            return repositoryRoot ?? currentDirectory;
        }
    }
    public static string? FindRepositoryRoot(string startPoint)
    {
        var currentFolder = new DirectoryInfo(startPoint);

        while (currentFolder is not null)
        {
            if (Directory.Exists(Path.Combine(currentFolder.FullName, SearchingClue)))
            {
                AnsiConsole.MarkupLine("[green]Repository root was found at path '" + currentFolder.FullName.EscapeMarkup() +
                    "'.[/]");
                return currentFolder.FullName;
            }

            currentFolder = currentFolder.Parent;
        }

        return null;
    }

    private static void CreateIfNeeded(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AnsiConsole.MarkupLine("[green]Folder was created at path '" + path.EscapeMarkup() +
                "', because it didn't existed before.[/]");
        }
    }

    private void UpdateGitignore(string path)
    {
        var gitignorePath = GetGitIgnorePath(path) ?? Path.Combine(path, GitIgnoreFileName);
        if (!File.Exists(gitignorePath))
        {
            File.Create(gitignorePath);
            AnsiConsole.MarkupLine("[green]File '.gitignore' was created at path '" + gitignorePath.EscapeMarkup() +
                "', because it didn't existed before.[/]");
        }

        AppendLinesToGitIgnore(gitignorePath);
    }

    public static string? GetGitIgnorePath(string repositoryRoot)
    {
        var gitIgnorePath = Path.Combine(repositoryRoot, GitIgnoreFileName);
        return File.Exists(gitIgnorePath) ? gitIgnorePath : null;
    }

    public void AppendLinesToGitIgnore(string gitIgnorePath)
    {
        var existingLines = File.ReadAllLines(gitIgnorePath);
        using var writer = File.AppendText(gitIgnorePath);

        var updated = false;
        foreach (var line in _linesForGitignore)
        {
            if (Array.Exists(existingLines, l => l.Trim() == line.Trim()))
            {
                continue;
            }

            writer.WriteLine(line);
            updated = true;
        }

        if (updated)
        {
            AnsiConsole.MarkupLine("[green]File '.gitignore' was updated.[/]");
        }
    }

    private static void CreateTeaPieFolder(string path)
    {
        var teaPieFolderPath = Path.Combine(path, Constants.TeaPieFolderName);

        if (!Directory.Exists(teaPieFolderPath))
        {
            Directory.CreateDirectory(teaPieFolderPath);
            AnsiConsole.MarkupLine("[green]TeaPie folder '.teapie' was created at path '" + teaPieFolderPath.EscapeMarkup() +
                "'.[/]");
        }
    }

    public sealed class Settings : LoggingSettings
    {
        [CommandArgument(0, "[path]")]
        [Description("Path at which .teapie folder should be created. If not specified, it attempts to find root of repository " +
            "(the one containing '.git' folder).")]
        public string? Path { get; init; }
    }
}
