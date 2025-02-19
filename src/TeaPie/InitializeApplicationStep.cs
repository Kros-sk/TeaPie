using Microsoft.Extensions.Logging;
using TeaPie.Http.Auth;
using TeaPie.Pipelines;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.Testing;

namespace TeaPie;

internal class InitializeApplicationStep(
    IPipeline pipeline,
    ITestResultsSummaryAccessor summaryAccessor,
    INuGetPackageHandler nuGetPackageHandler,
    IDefaultAuthProviderAccessor defaultAuthProviderAccessor,
    IAuthProviderRegistry authProviderRegistry)
    : IPipelineStep
{
    private readonly IPipeline _pipeline = pipeline;
    private readonly INuGetPackageHandler _nuGetPackageHandler = nuGetPackageHandler;
    private readonly ITestResultsSummaryAccessor _summaryAccessor = summaryAccessor;
    private readonly IDefaultAuthProviderAccessor _defaultAuthProviderAccessor = defaultAuthProviderAccessor;
    private readonly IAuthProviderRegistry _authProviderRegistry = authProviderRegistry;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        await DownloadAndInstallGlobalNuGetPackages();

        SetNoAuthProviderAsDefault();

        SetTestResultsSummaryObject(context.CollectionName);

        ResolveInitializationScript(context.CollectionStructure, context.ServiceProvider, context.Logger);
    }

    private void SetNoAuthProviderAsDefault()
        => _defaultAuthProviderAccessor.DefaultProvider = _authProviderRegistry.GetAuthProvider(AuthConstants.NoAuthKey);

    private async Task DownloadAndInstallGlobalNuGetPackages()
        => await _nuGetPackageHandler.HandleNuGetPackages(ScriptsConstants.DefaultNuGetPackages);

    private void SetTestResultsSummaryObject(string collectionName)
        => _summaryAccessor.Summary = new CollectionTestResultsSummary(collectionName);

    private void ResolveInitializationScript(
        IReadOnlyCollectionStructure collectionStructure,
        IServiceProvider serviceProvider,
        ILogger logger)
    {
        if (collectionStructure.HasInitializationScript)
        {
            var scriptExecutionContext = new ScriptExecutionContext(collectionStructure.InitializationScript);
            IPipelineStep[] steps =
                [.. ScriptStepsFactory.CreateStepsForScriptPreProcessAndExecution(serviceProvider, scriptExecutionContext)];

            _pipeline.InsertSteps(this, steps);

            logger.LogDebug("Using '{InitScriptPath}' as initialization script.",
                collectionStructure.InitializationScript.File.RelativePath);
        }
    }
}
