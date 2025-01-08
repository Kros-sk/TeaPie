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
            .WithStructureExplorationPipeline();
    }

    public sealed class Settings : SettingsWithLogging
    {
        [CommandArgument(0, "[path]")]
        [Description("Path to collection which will be explored. Defaults to the current directory.")]
        public string? Path { get; init; }
    }
}
