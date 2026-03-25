using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace TeaPie.DotnetTool;

internal sealed class InstallSkillsCommand : AsyncCommand<InstallSkillsCommand.Settings>
{
    private const string DefaultTargetPath = ".cursor/skills/teapie";
    private const string SkillsSourcePath = ".cursor/skills/teapie";
    private const string GitHubApiBaseUrl = "https://api.github.com/repos/Kros-sk/TeaPie/contents/";
    private const string UserAgentValue = "TeaPie-CLI";

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var targetPath = ResolveTargetPath(settings);

        AnsiConsole.MarkupLine($"[blue]Installing TeaPie skills into '{targetPath.EscapeMarkup()}'...[/]");

        using var httpClient = CreateHttpClient();

        try
        {
            var downloadedCount = await DownloadDirectoryAsync(httpClient, SkillsSourcePath, targetPath, settings.Force);

            AnsiConsole.MarkupLine(
                $"[green]Successfully installed {downloadedCount} skill file(s) into '{targetPath.EscapeMarkup()}'.[/]");
        }
        catch (HttpRequestException ex)
        {
            AnsiConsole.MarkupLine(
                $"[red]Failed to download skills from GitHub: {ex.Message.EscapeMarkup()}[/]");
            return 1;
        }

        return 0;
    }

    private static string ResolveTargetPath(Settings settings)
    {
        if (!string.IsNullOrEmpty(settings.Path))
        {
            return Path.GetFullPath(settings.Path);
        }

        return Path.GetFullPath(DefaultTargetPath);
    }

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentValue);
        return httpClient;
    }

    private static async Task<int> DownloadDirectoryAsync(
        HttpClient httpClient,
        string sourcePath,
        string targetPath,
        bool force)
    {
        var url = GitHubApiBaseUrl + sourcePath;
        var items = await httpClient.GetFromJsonAsync<GitHubContentItem[]>(url)
            ?? throw new HttpRequestException($"Failed to retrieve contents from '{url}'.");

        var downloadedCount = 0;

        foreach (var item in items)
        {
            if (item.Type == "file")
            {
                var downloaded = await DownloadFileAsync(httpClient, item, targetPath, sourcePath, force);
                if (downloaded)
                {
                    downloadedCount++;
                }
            }
            else if (item.Type == "dir")
            {
                downloadedCount += await DownloadDirectoryAsync(httpClient, item.Path, targetPath, force);
            }
        }

        return downloadedCount;
    }

    private static async Task<bool> DownloadFileAsync(
        HttpClient httpClient,
        GitHubContentItem item,
        string targetBasePath,
        string sourceBasePath,
        bool force)
    {
        var relativePath = item.Path[sourceBasePath.Length..].TrimStart('/');
        var targetFilePath = Path.Combine(targetBasePath, relativePath);

        if (File.Exists(targetFilePath) && !force)
        {
            AnsiConsole.MarkupLine($"[yellow]Skipping existing file: '{relativePath.EscapeMarkup()}' (use --force to overwrite).[/]");
            return false;
        }

        var directory = Path.GetDirectoryName(targetFilePath);
        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (string.IsNullOrEmpty(item.DownloadUrl))
        {
            AnsiConsole.MarkupLine($"[yellow]Skipping '{relativePath.EscapeMarkup()}': no download URL available.[/]");
            return false;
        }

        var content = await httpClient.GetByteArrayAsync(item.DownloadUrl);
        await File.WriteAllBytesAsync(targetFilePath, content);

        AnsiConsole.MarkupLine($"[green]Downloaded: '{relativePath.EscapeMarkup()}'[/]");
        return true;
    }

    internal sealed class Settings : CommandSettings
    {
        [CommandOption("-p|--path")]
        [Description("Target directory where skills will be installed. " +
            "Defaults to '.cursor/skills/teapie' relative to the current directory.")]
        public string? Path { get; init; }

        [CommandOption("-f|--force")]
        [DefaultValue(false)]
        [Description("Overwrite existing skill files.")]
        public bool Force { get; init; }
    }

    private sealed class GitHubContentItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("download_url")]
        public string? DownloadUrl { get; set; }
    }
}
