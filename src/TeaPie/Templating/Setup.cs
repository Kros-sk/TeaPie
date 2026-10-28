using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace TeaPie.Templating;

internal static class Setup
{
    public static IServiceCollection AddTemplating(this IServiceCollection services)
    {
        services.AddSingleton<ILoopBlockScanner, LoopBlockScanner>();
        services.AddSingleton<ILoopBodyMasker, LoopBodyMasker>();
        services.AddSingleton<ICollectionSourceResolver, CollectionSourceResolver>();
        services.AddSingleton<IVariablesFluidModelBuilder, VariablesFluidModelBuilder>();
        services.AddSingleton(new TemplatingLimits());
        services.AddSingleton<ITemplateExpander, TemplateExpander>();

        // Resolved on first use, so that a console silenced by '--quiet' is the one that gets used.
        services.AddSingleton<IExpandedContentDumper>(_ => new ExpandedContentDumper(AnsiConsole.Console));

        return services;
    }
}
