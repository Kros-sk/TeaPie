using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.DotnetTool;

internal sealed class TestCommand : ApplicationCommandBase<TestCommand.Settings>
{
    protected override ApplicationBuilder ConfigureApplication(Settings settings)
    {
        var pathToLogFile = settings.LogFile ?? string.Empty;
        var logLevel = Helper.ResolveLogLevel(settings);
        var path = PathResolver.Resolve(settings.Path, Directory.GetCurrentDirectory());
        var pathToRequestsLogFile = settings.RequestsLogFile ?? string.Empty;

        var appBuilder = ApplicationBuilder.Create(path.IsCollectionPath());

        appBuilder
            .WithPath(path)
            .WithTemporaryPath(settings.TemporaryPath ?? string.Empty)
            .WithLogging(logLevel, pathToLogFile, settings.LogFileLogLevel, pathToRequestsLogFile, settings.UseTreeLogging)
            .WithEnvironment(settings.Environment ?? string.Empty)
            .WithEnvironmentFile(PathResolver.Resolve(settings.EnvironmentFilePath, string.Empty))
            .WithReportFile(PathResolver.Resolve(settings.ReportFilePath, string.Empty))
            .WithInitializationScript(PathResolver.Resolve(settings.InitializationScriptPath, string.Empty))
            .WithVariablesCaching(!settings.NoVariablesCaching);

        if (settings.ShouldDumpExpanded)
        {
            appBuilder.WithExpandedContentDump(PathResolver.Resolve(settings.DumpExpandedDirectory, string.Empty));
        }

        appBuilder.WithDefaultPipeline();

        return appBuilder;
    }

    public sealed class Settings : LoggingSettings
    {
        [CommandArgument(0, "[path]")]
        [Description("Path to the collection, test case (.http) or single-file test case (.tp) which will be tested. Defaults to the current directory.")]
        public string? Path { get; init; }

        [CommandOption("--temp-path")]
        [Description("Temporary path for the application. Defaults to the system's temporary folder with a TeaPie sub-folder " +
            "if no path is provided.")]
        public string? TemporaryPath { get; init; }

        [CommandOption("-e|--env|--environment")]
        [Description("Name of the environment on which application will be run.")]
        public string? Environment { get; init; }

        [CommandOption("--env-file|--environment-file")]
        [Description("Path to file, which contains definitions of available environments. If not provided, " +
            "the tool will use the first matching 'env.json' file found in '.teapie' folder or the collection " +
            "(respectively parent folder of test case).")]
        public string? EnvironmentFilePath { get; init; }

        [CommandOption("-r|--report-file")]
        [Description("Path to file, which will be used for test results summary report generation. " +
            "If this option is not used, no report to file is generated.")]
        public string? ReportFilePath { get; init; }

        [CommandOption("-i|--init-script|--initialization-script")]
        [Description("Path to script, which will be used for initialization before the first test-case execution. " +
            "If not provided, the tool will use the first matching 'init.csx' file found in '.teapie' folder or the collection" +
            " (respectively parent folder of test case).")]
        public string? InitializationScriptPath { get; init; }

        [CommandOption("--no-cache-vars|--no-cache-variables")]
        [DefaultValue(false)]
        [Description("Disables loading variables from file and caching them to file.")]
        public bool NoVariablesCaching { get; init; }

        [CommandOption("--dump-expanded")]
        [DefaultValue(false)]
        [Description("Writes the content of each templated requests file to the console after its templates are " +
            "expanded. Can't be combined with '-q|--quiet', which silences the console.")]
        public bool DumpExpanded { get; init; }

        [CommandOption("--dump-expanded-dir <DIRECTORY>")]
        [Description("Writes the content of each templated requests file after its templates are expanded into the " +
            "given directory instead of the console, keeping its relative path. The directory must be outside the " +
            "tested collection. Turns the dump on by itself, '--dump-expanded' is not needed.")]
        public string? DumpExpandedDirectory { get; init; }

        internal bool ShouldDumpExpanded => DumpExpanded || !string.IsNullOrEmpty(DumpExpandedDirectory);

        // Validated before '--quiet' silences the console, so the error is still shown.
        public override ValidationResult Validate()
            => IsQuiet && DumpExpanded && string.IsNullOrEmpty(DumpExpandedDirectory)
                ? ValidationResult.Error(
                    "Option '--dump-expanded' writes to the console, which '-q|--quiet' silences. " +
                    "Use '--dump-expanded-dir <DIRECTORY>' to dump into a directory instead.")
                : base.Validate();
    }
}
