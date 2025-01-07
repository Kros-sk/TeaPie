using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace TeaPie.DotnetTool;

internal class GenerateCommand : Command<GenerateCommand.Settings>
{
    public override int Execute(CommandContext context, Settings settings)
    {
        var path = settings.Path ?? Directory.GetCurrentDirectory();

        GeneratePreRequestFile(path, settings);
        GenerateRequestFile(path, settings.Name);
        GeneratePostResponseFile(path, settings);

        ReportSuccessfullCreation(settings);

        return 0;
    }

    private static string GenerateRequestFile(string path, string name)
        => GenerateFile(path, name, GetRequestFileName);

    private static void GeneratePreRequestFile(string path, Settings settings)
        => GenerateScript(settings.HasPreRequestScript, path, settings.Name, GetPreRequestFileName);

    private static void GeneratePostResponseFile(string path, Settings settings)
        => GenerateScript(settings.HasPostResponseScript, path, settings.Name, GetPostResponseFileName);

    private static void GenerateScript(bool condition, string path, string name, Func<string, string> nameGetter)
    {
        if (condition)
        {
            GenerateFile(path, name, nameGetter);
        }
    }

    private static string GenerateFile(string path, string name, Func<string, string> nameGetter)
    {
        var filePath = Path.Combine(path, nameGetter(name));
        File.Create(filePath);

        return filePath;
    }

    private static string GetRequestFileName(string name)
        => name + Constants.RequestSuffix + Constants.RequestFileExtension;

    private static string GetPreRequestFileName(string name)
        => name + Constants.PreRequestSuffix + Constants.ScriptFileExtension;

    private static string GetPostResponseFileName(string name)
        => name + Constants.PostResponseSuffix + Constants.ScriptFileExtension;

    private static void ReportSuccessfullCreation(Settings settings)
    {
        const string beginningOfSentence = "[green]Test case [/]";
        var testCaseName = $"[white]'{settings.Name}'[/]";
        const string endOfSentence = "[green] was successfully generated.[/]";
        var description = GetDescription(settings);

        AnsiConsole.Markup(beginningOfSentence + testCaseName + endOfSentence + description);
    }

    private static string GetDescription(Settings settings)
    {
        var description = string.Empty;
        if (settings.HasPreRequestScript || settings.HasPreRequestScript)
        {
            var hasPreReq = "Pre-Request: " + (settings.HasPreRequestScript ? "YES" : "NO");
            var hasPostRes = "Post-Response: " + (settings.HasPostResponseScript ? "YES" : "NO");
            description = $"[grey] ({hasPreReq} | {hasPostRes})[/]";
        }
        return description;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<name>")]
        [Description("Name of test case to be generated.")]
        public string Name { get; init; } = "testCase";

        [CommandArgument(1, "[path]")]
        [Description("Path on which entry will be created.")]
        public string? Path { get; init; }

        [CommandOption("-i|--init|--pre-req")]
        [DefaultValue(true)]
        [Description("Indicates whether to generate pre-request script (with '-init' suffix).")]
        public bool HasPreRequestScript { get; init; }

        [CommandOption("-t|--test|--post-res")]
        [DefaultValue(true)]
        [Description("Indicates whether to generate post-response script (with '-test' suffix).")]
        public bool HasPostResponseScript { get; init; }
    }
}
