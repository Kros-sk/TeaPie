using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using TeaPie.Reporting;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.DotnetTool;

internal class CompileScriptCommand : ApplicationCommandBase<CompileScriptCommand.Settings>
{
    protected override ApplicationBuilder ConfigureApplication(Settings settings)
    {
        var pathToLogFile = settings.LogFile ?? string.Empty;
        var logLevel = Helper.ResolveLogLevel(settings);
        var path = PathResolver.Resolve(settings.Path, string.Empty);

        var appBuilder = ApplicationBuilder.Create(path.IsCollectionPath());

        appBuilder
            .WithPath(path)
            .WithTemporaryPath(string.Empty)
            .WithScriptCompilationPipeline(path)
            .WithLogging(logLevel, pathToLogFile, settings.LogFileLogLevel);

        return appBuilder;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var result = await BuildApplication(settings).Run(new CancellationToken());
        InterpretResult(result);
        return result;
    }

    private static void InterpretResult(int result)
    {
        if (result == 0)
        {
            AnsiConsole.MarkupLine($"[green]{GetSuccessEmoji()}Compilation of the script was successfull.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]{GetFailEmoji()}Compilation of the script failed.[/]");
        }
    }

    private static string GetSuccessEmoji()
        => CompatibilityChecker.SupportsEmoji
            ? Emoji.Known.CheckMarkButton + " "
            : string.Empty;

    private static string GetFailEmoji()
        => CompatibilityChecker.SupportsEmoji
            ? Emoji.Known.CrossMark + " "
            : string.Empty;

    public sealed class Settings : LoggingSettings
    {
        [CommandArgument(0, "<path>")]
        [Description("Path to script which should be compiled.")]
        public string? Path { get; init; }
    }
}
