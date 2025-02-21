using Spectre.Console.Cli;
using System.ComponentModel;

namespace TeaPie.DotnetTool;

internal class ExploreCommand : ApplicationCommandBase<ExploreCommand.Settings>
{
    protected override void ConfigureApplication(ApplicationBuilder appBuilder, Settings settings)
    {
        var pathToLogFile = settings.LogFile ?? string.Empty;
        var logLevel = ResolveLogLevel(settings);

        appBuilder
            .WithPath(PathResolver.Resolve(settings.Path, string.Empty))
            .WithLogging(logLevel, pathToLogFile, settings.LogFileLogLevel)
            .WithEnvironmentFile(PathResolver.Resolve(settings.EnvironmentFile, string.Empty))
            .WithStructureExplorationPipeline();
    }

    public sealed class Settings : SettingsWithLogging
    {
        [CommandArgument(0, "[path]")]
        [Description("Path to collection which will be explored. Defaults to the current directory.")]
        public string? Path { get; init; }

        [CommandOption("--env-file|--environment-file")]
        [Description("Path to file, which contains definitions of available environments. If this option is not used, " +
            "first found file within collection with name '<collection-name>-env.json' is used.")]
        public string? EnvironmentFile { get; init; }
    }
}
