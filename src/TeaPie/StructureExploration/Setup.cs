using Microsoft.Extensions.DependencyInjection;
using TeaPie.Reporting;

namespace TeaPie.StructureExploration;

internal static class Setup
{
    public static IServiceCollection AddStructureExploration(
        this IServiceCollection services, bool isCollectionRun)
    {
        services.AddSingleton<ITreeStructureRenderer, SpectreConsoleTreeStructureRenderer>();

        return isCollectionRun
            ? services.AddSingleton<IStructureExplorer, CollectionStructureExplorer>()
            : services.AddSingleton<IStructureExplorer, TestCaseStructureExplorer>();
    }
}
