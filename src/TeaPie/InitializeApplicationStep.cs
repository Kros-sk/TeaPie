using TeaPie.Pipelines;
using TeaPie.Scripts;

namespace TeaPie;

internal class InitializeApplicationStep(INuGetPackageHandler nuGetPackageHandler) : IPipelineStep
{
    private readonly INuGetPackageHandler _nuGetPackageHandler = nuGetPackageHandler;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
        => await _nuGetPackageHandler.HandleNuGetPackages(ScriptsConstants.DefaultNuGetPackages);
}
