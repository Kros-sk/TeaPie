using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace TeaPie.DotnetTool;

internal class LoggingSettings : CommandSettings
{
    [CommandOption("--log-file")]
    [Description("Path to the file where all logs will be saved.")]
    public string? LogFile { get; init; }

    [CommandOption("--log-file-log-level")]
    [Description("Log level for the log file (if the log file path is specified). " +
        "Supported levels: Trace, Debug, Information, Warning, Error, Critical, None.")]
    public LogLevel LogFileLogLevel { get; init; } = LogLevel.Trace;

    [CommandOption("--request-response-log-file")]
    [Description("Path to the file where only request/response logs will be saved.")]
    public string? RequestResponseLogFile { get; init; }

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
    [Description("Runs command silently, without displaying any output. Default: false.")]
    public bool IsQuiet { get; init; }
}
