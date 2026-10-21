using Microsoft.Extensions.DependencyInjection;

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

        return services;
    }
}
