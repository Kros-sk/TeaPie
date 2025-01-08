using Spectre.Console.Cli;
using System.ComponentModel;

namespace TeaPie.DotnetTool;

internal class ExploreCommand : ApplicationCommandBase<ExploreCommand.Settings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        => await base.ExecuteAsync(context, settings);

    protected override void ConfigureApplication(ApplicationBuilder appBuilder, Settings settings)
    {
        var pathToLogFile = settings.LogFile ?? string.Empty;
        var logLevel = ResolveLogLevel(settings);

        appBuilder
            .WithPath(settings.Path ?? string.Empty)
            .WithLogging(logLevel, pathToLogFile, settings.LogFileLogLevel)
            .WithStructureExplorationPipeline();
    }

    public sealed class Settings : SettingsWithLogging
    {
        [CommandArgument(0, "[path]")]
        [Description("Path to the collection which will be explored. Defaults to the current directory.")]
        public string? Path { get; init; }
    }
}
