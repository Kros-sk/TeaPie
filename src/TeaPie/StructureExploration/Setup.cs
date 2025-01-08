using Microsoft.Extensions.DependencyInjection;
using TeaPie.Reporting;

namespace TeaPie.StructureExploration;

internal static class Setup
{
    public static IServiceCollection AddStructureExploration(this IServiceCollection services)
    {
        services.AddSingleton<IStructureExplorer, StructureExplorer>();
        services.AddSingleton<ITreeStructureRenderer, SpectreConsoleTreeStructureRenderer>();
        return services;
    }
}
