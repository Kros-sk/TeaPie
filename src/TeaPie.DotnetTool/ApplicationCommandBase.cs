using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace TeaPie.DotnetTool;
internal abstract class ApplicationCommandBase<TSettings> : AsyncCommand<TSettings> where TSettings : SettingsWithLogging
{
    public override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
    {
        var app = BuildApplication(settings);
        await app.Run(new CancellationToken());

        return 0;
    }

    protected Application BuildApplication(TSettings settings)
    {
        var appBuilder = ApplicationBuilder.Create();

        ConfigureApplication(appBuilder, settings);

        return appBuilder.Build();
    }

    protected abstract void ConfigureApplication(ApplicationBuilder appBuilder, TSettings settings);

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
