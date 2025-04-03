using TeaPie.StructureExploration;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.Scripts;

internal interface IScriptLineResolversProvider
{
    IReadOnlyList<IScriptLineResolver> GetAvailableResolvers();
}

internal class ScriptLineResolversProvider(
    INuGetPackageHandler nugetPackagesHandler,
    IPathResolver pathResolver,
    TemporaryPathResolver tempPathResolver,
    IExternalFileRegistry externalFileRegistry,
    IPathProvider pathProvider) : IScriptLineResolversProvider
{
    private readonly List<IScriptLineResolver> _scriptLineResolvers =
    [
        new NuGetDirectiveLineResolver(nugetPackagesHandler),
        new LoadDirectiveLineResolver(pathResolver, tempPathResolver, externalFileRegistry, pathProvider),
        new PlainLineResolver()
    ];

    public IReadOnlyList<IScriptLineResolver> GetAvailableResolvers() => _scriptLineResolvers;
}
