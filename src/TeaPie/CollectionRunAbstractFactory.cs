using Microsoft.Extensions.DependencyInjection;
using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.StructureExploration;

namespace TeaPie;

internal class CollectionRunAbstractFactory : IApplicationAbstractFactory
{
    public IServiceCollection AddStructureExploration(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IStructureExplorer, CollectionStructureExplorer>();
        serviceCollection.AddSingleton<ITreeStructureRenderer, SpectreConsoleTreeStructureRenderer>();

        return serviceCollection;
    }

    public IServiceCollection AddTestResultsSummaryReporter(IServiceCollection serviceCollection)
        => serviceCollection.AddSingleton<ITestResultsSummaryReporter, CollectionTestResultsSummaryReporter>();

    public void CheckPath(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new InvalidOperationException("Unable to start collection run, if path doesn't refer to existing folder.");
        }
    }

    public Func<IServiceProvider, IPipelineStep[]> GetDefaultPipelineBuildFunction()
        => ApplicationStepsFactory.CreateDefaultPipelineSteps;

    public Func<IServiceProvider, IPipelineStep[]> GetStructureExplorationPipelineBuildFunction()
        => ApplicationStepsFactory.CreateStructureExplorationSteps;
}
