using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace TeaPie.DotnetTool;

internal sealed class TestCommand : AsyncCommand<TestCommand.Settings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var app = BuildApplication(settings);
        await app.Run(new CancellationToken());

        return 0;
    }

    private static Application BuildApplication(Settings settings)
    {
        var appBuilder = ApplicationBuilder.Create();

        Configure(appBuilder, settings);

        return appBuilder.Build();
    }

    private static void Configure(ApplicationBuilder appBuilder, Settings settings)
    {
        var path = settings.CollectionPath ?? Directory.GetCurrentDirectory();
        if (settings.TempPath is not null)
        {
            appBuilder.WithTemporaryPath(settings.TempPath);
        }

        var pathToLogFile = settings.LogFile ?? string.Empty;
        var logLevel = ResolveLogLevel(settings);

        appBuilder
            .WithPath(path)
            .AddLogging(logLevel, pathToLogFile, settings.LogFileLogLevel);
    }

    private static LogLevel ResolveLogLevel(Settings settings)
        => settings.IsQuiet
            ? Silence()
            : settings.IsDebug
                ? LogLevel.Debug
                : settings.IsVerbose
                    ? LogLevel.Trace
                    : settings.LogLevel;

    private static LogLevel Silence()
    {
        Console.SetOut(TextWriter.Null);
        Console.SetError(TextWriter.Null);

        AnsiConsole.Console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(TextWriter.Null)
        });

        return LogLevel.None;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[collectionPath]")]
        [Description("Path to the collection. Defaults to the current directory.")]
        public string? CollectionPath { get; init; }

        [CommandOption("--temp-path")]
        [Description("Temporary path for the application. Defaults to the system's temporary folder with a TeaPie sub-folder " +
            "if no path is provided.")]
        public string? TempPath { get; init; }

        #region Logging
        [CommandOption("--log-file")]
        [Description("Path to the file where all application logs will be saved.")]
        public string? LogFile { get; init; }

        [CommandOption("--log-file-log-level")]
        [Description("Log level for the log file (if a file path is specified). " +
            "Supported levels: Trace, Debug, Information, Warning, Error, Critical, None.")]
        public LogLevel LogFileLogLevel { get; init; } = LogLevel.Information;

        [CommandOption("-l|--log-level")]
        [Description("Log level for console output. " +
            "Supported levels: Trace, Debug, Information, Warning, Error, Critical, None.")]
        public LogLevel LogLevel { get; init; } = LogLevel.Information;

        [CommandOption("-d|--debug")]
        [DefaultValue(false)]
        [Description("Displays debug information. Default: false.")]
        public bool IsDebug { get; init; }

        [CommandOption("-v|--verbose")]
        [DefaultValue(false)]
        [Description("Displays all available information, including debug details. Default: false.")]
        public bool IsVerbose { get; init; }

        [CommandOption("-q|--quiet")]
        [DefaultValue(false)]
        [Description("Runs the application silently, without displaying any output. Default: false.")]
        public bool IsQuiet { get; init; }
        #endregion
    }
}
