using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using TeaPie.Reporting;
using TeaPie.StructureExploration.Paths;
using TeaPie.TestCases;

namespace TeaPie.DotnetTool;

internal class CompileScriptCommand : ApplicationCommandBase<CompileScriptCommand.Settings>
{
    protected override ApplicationBuilder ConfigureApplication(Settings settings)
    {
        var pathToLogFile = settings.LogFile ?? string.Empty;
        var logLevel = Helper.ResolveLogLevel(settings);
        var path = PathResolver.Resolve(settings.Path, Directory.GetCurrentDirectory());

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
        var path = PathResolver.Resolve(settings.Path, Directory.GetCurrentDirectory());

        if (!path.IsTpFile())
        {
            var result = await BuildApplication(settings).Run(CancellationToken.None);
            InterpretResult(result);
            return result;
        }

        return await CompileTpScripts(path, settings);
    }

    private async Task<int> CompileTpScripts(string path, Settings settings)
    {
        AnsiConsole.MarkupLine($"[yellow]Compiling scripts from .tp file:[/] [white]'{path.EscapeMarkup()}'[/]");

        var content = File.ReadAllText(path);
        var parser = new TpFileParser();
        var parsingContext = new TpParsingContext(content, Path.GetFileNameWithoutExtension(path));

        try
        {
            parser.Parse(parsingContext);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]{GetFailEmoji()}Failed to parse .tp file: {ex.Message.EscapeMarkup()}[/]");
            return 1;
        }

        var overallSuccess = true;

        foreach (var def in parsingContext.Definitions)
        {
            AnsiConsole.MarkupLine($"\n[bold]Test case:[/] [white]'{def.Name.EscapeMarkup()}'[/]");

            if (!string.IsNullOrWhiteSpace(def.InitContent))
            {
                overallSuccess &= await CompileSection(path, def.InitContent, $"{def.Name} - INIT", settings);
            }

            if (!string.IsNullOrWhiteSpace(def.TestContent))
            {
                overallSuccess &= await CompileSection(path, def.TestContent, $"{def.Name} - TEST", settings);
            }

            if (string.IsNullOrWhiteSpace(def.InitContent) && string.IsNullOrWhiteSpace(def.TestContent))
            {
                AnsiConsole.MarkupLine("[grey] No script sections to compile (HTTP only).[/]");
            }
        }

        AnsiConsole.WriteLine();
        InterpretResult(overallSuccess ? 0 : 1);
        return overallSuccess ? 0 : 1;
    }

    private async Task<bool> CompileSection(string tpPath, string scriptContent, string label, Settings settings)
    {
        AnsiConsole.Markup($"  [grey]Compiling section:[/] [white]{label.EscapeMarkup()}[/] ... ");

        var logLevel = Helper.ResolveLogLevel(settings);
        var app = ApplicationBuilder.Create(false)
            .WithPath(tpPath)
            .WithTemporaryPath(string.Empty)
            .WithScriptCompilationPipeline(tpPath)
            .WithScriptContent(scriptContent)
            .WithLogging(logLevel, settings.LogFile ?? string.Empty, settings.LogFileLogLevel)
            .Build();

        var result = await app.Run(CancellationToken.None);

        if (result == 0)
        {
            AnsiConsole.MarkupLine($"[green]{GetSuccessEmoji()}OK[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]{GetFailEmoji()}FAILED[/]");
        }

        return result == 0;
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
        [Description("Path to script (.csx) or test case file (.tp) which should be compiled.")]
        public string? Path { get; init; }
    }
}
