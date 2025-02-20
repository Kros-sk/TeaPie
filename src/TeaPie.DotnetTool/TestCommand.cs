using Spectre.Console.Cli;
using System.ComponentModel;
using TeaPie.Application;

namespace TeaPie.DotnetTool;

internal sealed class TestCommand : ApplicationCommandBase<TestCommand.Settings>
{
    protected override void ConfigureApplication(ApplicationBuilder appBuilder, Settings settings)
    {
        var pathToLogFile = settings.LogFile ?? string.Empty;
        var logLevel = ResolveLogLevel(settings);

        appBuilder
            .WithPath(PathResolver.Resolve(settings.Path, string.Empty))
            .WithTemporaryPath(settings.TemporaryPath ?? string.Empty)
            .WithLogging(logLevel, pathToLogFile, settings.LogFileLogLevel)
            .WithEnvironment(settings.Environment ?? string.Empty)
            .WithEnvironmentFile(PathResolver.Resolve(settings.EnvironmentFilePath, string.Empty))
            .WithReportFile(PathResolver.Resolve(settings.ReportFilePath, string.Empty))
            .WithInitializationScript(PathResolver.Resolve(settings.InitializationScriptPath, string.Empty))
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

        [CommandOption("-e|--env|--environment")]
        [Description("Name of the environment on which collection will be run.")]
        public string? Environment { get; init; }

        [CommandOption("--env-file|--environment-file")]
        [Description("Path to file, which contains definitions of available environments. If this option is not used, " +
            "first found file within collection with name '<collection-name>-env.json' is used.")]
        public string? EnvironmentFilePath { get; init; }

        [CommandOption("-r|--report-file")]
        [Description("Path to file, which will be used for test results summary report generation. " +
            "If this option is not used, no report to file is generated.")]
        public string? ReportFilePath { get; init; }

        [CommandOption("-i|--init-script|--initialization-script")]
        [Description("Path to script, which will be used for initialization before the first test-case execution. " +
            "If this option is not used, first found file within collection with name 'init.csx' is used.")]
        public string? InitializationScriptPath { get; init; }
    }
}
