using Spectre.Console.Cli;
using System.ComponentModel;

namespace TeaPie.DotnetTool;

internal sealed class TestCommand : ApplicationCommandBase<TestCommand.Settings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        => await base.ExecuteAsync(context, settings);

    protected override void ConfigureApplication(ApplicationBuilder appBuilder, Settings settings)
    {
        var pathToLogFile = settings.LogFile ?? string.Empty;
        var logLevel = ResolveLogLevel(settings);

        appBuilder
            .WithPath(PathResolver.Resolve(settings.Path, string.Empty))
            .WithTemporaryPath(settings.TemporaryPath ?? string.Empty)
            .WithLogging(logLevel, pathToLogFile, settings.LogFileLogLevel)
            .WithDefaultPipeline();
    }

    public sealed class Settings : SettingsWithLogging
    {
        [CommandArgument(0, "[path]")]
        [Description("Path to the collection which will be tested. Defaults to the current directory.")]
        public string? Path { get; init; }

        [CommandOption("--temp-path")]
        [Description("Temporary path for the application. Defaults to the system's temporary folder with a TeaPie sub-folder " +
            "if no path is provided.")]
        public string? TemporaryPath { get; init; }
    }
}
