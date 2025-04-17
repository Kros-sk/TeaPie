using Spectre.Console.Cli;

namespace TeaPie.DotnetTool;

internal abstract class ApplicationCommandBase<TSettings> : AsyncCommand<TSettings> where TSettings : LoggingSettings
{
    public override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
        => await BuildApplication(settings).Run(new CancellationToken());

    protected Application BuildApplication(TSettings settings)
        => ConfigureApplication(settings).Build();

    protected abstract ApplicationBuilder ConfigureApplication(TSettings settings);
}
