using Microsoft.Extensions.DependencyInjection;
using TeaPie.Pipelines;

namespace TeaPie;

internal interface IApplicationAbstractFactory
{
    void CheckPath(string path);

    Func<IServiceProvider, IPipelineStep[]> GetDefaultPipelineBuildFunction();

    Func<IServiceProvider, IPipelineStep[]> GetStructureExplorationPipelineBuildFunction();

    IServiceCollection AddStructureExploration(IServiceCollection serviceCollection);

    IServiceCollection AddTestResultsSummaryReporter(IServiceCollection serviceCollection);
}
