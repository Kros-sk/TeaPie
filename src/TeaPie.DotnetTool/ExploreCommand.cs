using Spectre.Console.Cli;
using System.ComponentModel;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.DotnetTool;

internal class ExploreCommand : ApplicationCommandBase<ExploreCommand.Settings>
{
    protected override ApplicationBuilder ConfigureApplication(Settings settings)
    {
        var pathToLogFile = settings.LogFile ?? string.Empty;
        var logLevel = Helper.ResolveLogLevel(settings);
        var path = PathResolver.Resolve(settings.Path, Directory.GetCurrentDirectory());

        var appBuilder = ApplicationBuilder.Create(path.IsCollectionPath());

        appBuilder
            .WithPath(path)
            .WithLogging(
                logLevel,
                pathToLogFile,
                settings.LogFileLogLevel,
                settings.CategorizedJsonLogFile ?? string.Empty)
            .WithEnvironmentFile(PathResolver.Resolve(settings.EnvironmentFile, string.Empty))
            .WithInitializationScript(PathResolver.Resolve(settings.InitializationScriptPath, string.Empty))
            .WithStructureExplorationPipeline();

        return appBuilder;
    }

    public sealed class Settings : LoggingSettings
    {
        [CommandArgument(0, "[path]")]
        [Description("Path to collection or test case which will be explored. Defaults to the current directory.")]
        public string? Path { get; init; }

        [CommandOption("--env-file|--environment-file")]
        [Description("Path to file, which contains definitions of available environments. If this option is not used, " +
            "first found file within '.teapie' folder or collection (respectively parent folder of test case) " +
            "with name 'env.json' is used.")]
        public string? EnvironmentFile { get; init; }

        [CommandOption("-i|--init-script|--initialization-script")]
        [Description("Path to script, which will be used for initialization before the first test-case execution. " +
            "If this option is not used, first found file within '.teapie' folder or collection " +
            "(respectively parent folder of test case) with name 'init.csx' is used.")]
        public string? InitializationScriptPath { get; init; }
    }
}
