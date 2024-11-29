using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.StructureExploration;

internal static class Setup
{
    public static IServiceCollection AddStructureExploration(this IServiceCollection services)
    {
        services.AddSingleton<IStructureExplorer, StructureExplorer>();
        return services;
    }
}
