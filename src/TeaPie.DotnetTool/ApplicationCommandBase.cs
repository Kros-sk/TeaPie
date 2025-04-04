﻿using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace TeaPie.DotnetTool;

internal abstract class ApplicationCommandBase<TSettings> : AsyncCommand<TSettings> where TSettings : LoggingSettings
{
    public override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
        => await BuildApplication(settings).Run(new CancellationToken());

    protected Application BuildApplication(TSettings settings)
        => ConfigureApplication(settings).Build();

    protected abstract ApplicationBuilder ConfigureApplication(TSettings settings);

    protected static LogLevel ResolveLogLevel(TSettings settings)
        => settings switch
        {
            { IsQuiet: true } => Silence(),
            { IsDebug: true } => LogLevel.Debug,
            { IsVerbose: true } => LogLevel.Trace,
            _ => settings.LogLevel
        };

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
}
