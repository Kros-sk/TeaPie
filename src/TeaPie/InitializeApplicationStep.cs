using Microsoft.Extensions.Logging;
using TeaPie.Http.Auth;
using TeaPie.Http.Auth.OAuth2;
using TeaPie.Pipelines;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.StructureExploration.Paths;
using TeaPie.Testing;

namespace TeaPie;

internal class InitializeApplicationStep(
    IPipeline pipeline,
    ITestResultsSummaryAccessor summaryAccessor,
    INuGetPackageHandler nuGetPackageHandler,
    IAuthProviderAccessor authProviderAccessor,
    IAuthProviderRegistry authProviderRegistry,
    IPathProvider pathProvider,
    OAuth2Provider oAuth2Provider)
    : IPipelineStep
{
    private readonly IPipeline _pipeline = pipeline;
    private readonly INuGetPackageHandler _nuGetPackageHandler = nuGetPackageHandler;
    private readonly ITestResultsSummaryAccessor _summaryAccessor = summaryAccessor;
    private readonly IAuthProviderAccessor _authProviderAccessor = authProviderAccessor;
    private readonly IAuthProviderRegistry _authProviderRegistry = authProviderRegistry;
    private readonly OAuth2Provider _oAuth2Provider = oAuth2Provider;
    private readonly IPathProvider _pathProvider = pathProvider;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        DeleteOldTempFolderIfNeeded();

        await DownloadAndInstallGlobalNuGetPackages(context.Logger);

        ResolveAuthProviders(context.Logger);

        SetTestResultsSummaryObject(context.StructureName);

        ResolveInitializationScript(context.CollectionStructure, context.ServiceProvider, context.Logger);
    }

    private void DeleteOldTempFolderIfNeeded()
    {
        if (Directory.Exists(_pathProvider.TempFolderPath))
        {
            Directory.Delete(_pathProvider.TempFolderPath, true);
        }
    }

    private void ResolveAuthProviders(ILogger logger)
    {
        _authProviderAccessor.DefaultProvider = _authProviderRegistry.Get(AuthConstants.NoAuthKey);
        _authProviderAccessor.SetCurrentProviderToDefault();
        _authProviderRegistry.Register(AuthConstants.OAuth2Key, _oAuth2Provider);

        logger.LogTrace("Pre-defined auth providers were registered: {NoAuth}, {OAuth2}.",
            AuthConstants.NoAuthKey, AuthConstants.OAuth2Key);
    }

    private async Task DownloadAndInstallGlobalNuGetPackages(ILogger logger)
    {
        await _nuGetPackageHandler.HandleNuGetPackages(ScriptsConstants.DefaultNuGetPackages);

        logger.LogTrace("Default NuGet packages were successfully added: ({NuGetPackages})",
            string.Join(", ", ScriptsConstants.DefaultNuGetPackages.Select(x => $"{x.PackageName}, {x.Version}")));
    }

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
                collectionStructure.InitializationScript.File.GetDisplayPath());
        }
    }
}
