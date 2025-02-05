using TeaPie.Pipelines;
using TeaPie.Scripts;
using TeaPie.Testing;

namespace TeaPie;

internal class InitializeApplicationStep(ITestResultsSummaryAccessor summaryAccessor, INuGetPackageHandler nuGetPackageHandler)
    : IPipelineStep
{
    private readonly INuGetPackageHandler _nuGetPackageHandler = nuGetPackageHandler;
    private readonly ITestResultsSummaryAccessor _summaryAccessor = summaryAccessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        await _nuGetPackageHandler.HandleNuGetPackages(ScriptsConstants.DefaultNuGetPackages);
        _summaryAccessor.Summary = new CollectionTestResultsSummary();
    }
}
