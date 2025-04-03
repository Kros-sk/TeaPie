using Microsoft.Extensions.DependencyInjection;
using TeaPie.Reporting;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.StructureExploration;

internal static class Setup
{
    public static IServiceCollection AddStructureExploration(
        this IServiceCollection services, bool isCollectionRun)
    {
        services.AddSingleton<ITreeStructureRenderer, SpectreConsoleTreeStructureRenderer>();

        services.AddSingleton<IExternalFileRegistry, ExternalFilesRegistry>();

        services.AddPaths();

        return isCollectionRun
            ? services.AddSingleton<IStructureExplorer, CollectionStructureExplorer>()
            : services.AddSingleton<IStructureExplorer, TestCaseStructureExplorer>();
    }
}
