using Microsoft.Extensions.Logging;
using TeaPie.Http.Auth;
using TeaPie.Http.Auth.OAuth2;
using TeaPie.Pipelines;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.Testing;

namespace TeaPie;

internal class InitializeApplicationStep(
    IPipeline pipeline,
    ITestResultsSummaryAccessor summaryAccessor,
    INuGetPackageHandler nuGetPackageHandler,
    IAuthProviderAccessor defaultAuthProviderAccessor,
    IAuthProviderRegistry authProviderRegistry,
    OAuth2Provider oAuth2Provider)
    : IPipelineStep
{
    private readonly IPipeline _pipeline = pipeline;
    private readonly INuGetPackageHandler _nuGetPackageHandler = nuGetPackageHandler;
    private readonly ITestResultsSummaryAccessor _summaryAccessor = summaryAccessor;
    private readonly IAuthProviderAccessor _defaultAuthProviderAccessor = defaultAuthProviderAccessor;
    private readonly IAuthProviderRegistry _authProviderRegistry = authProviderRegistry;
    private readonly OAuth2Provider _oAuth2Provider = oAuth2Provider;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        await DownloadAndInstallGlobalNuGetPackages();

        ResolveAuthProviders();

        SetTestResultsSummaryObject(context.CollectionName);

        ResolveInitializationScript(context.CollectionStructure, context.ServiceProvider, context.Logger);
    }

    private void ResolveAuthProviders()
    {
        _defaultAuthProviderAccessor.DefaultProvider = _authProviderRegistry.Get(AuthConstants.NoAuthKey);
        _defaultAuthProviderAccessor.SetCurrentProviderToDefault();
        _authProviderRegistry.Register(AuthConstants.OAuth2Key, _oAuth2Provider);
    }

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
