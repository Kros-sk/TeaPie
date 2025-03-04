using Microsoft.Extensions.DependencyInjection;
using TeaPie.Pipelines;

namespace TeaPie;

internal class SingleTestCaseRunAbstractFactory : IApplicationAbstractFactory
{
    public IServiceCollection AddStructureExploration(IServiceCollection serviceCollection) => throw new NotImplementedException();
    public IServiceCollection AddTestResultsSummaryReporter(IServiceCollection serviceCollection) => throw new NotImplementedException();
    public void CheckPath(string path) => throw new NotImplementedException();
    public Func<IServiceProvider, IPipelineStep[]> GetDefaultPipelineBuildFunction() => throw new NotImplementedException();
    public Func<IServiceProvider, IPipelineStep[]> GetStructureExplorationPipelineBuildFunction() => throw new NotImplementedException();
}
