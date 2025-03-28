using TeaPie.Scripts;

namespace TeaPie.StructureExploration.Paths;

internal class TeaPieFolderPathResolver(IPathProvider pathProvider) : IPathResolver
{
    private readonly IPathProvider _pathProvider = pathProvider;

    public bool CanResolve(string path)
        => path.TrimStart().StartsWith(ScriptsConstants.TeaPieFolderPathReference) &&
            !string.IsNullOrEmpty(_pathProvider.TeaPieFolderPath);

    public string ResolvePath(string path, string basePath)
        => path.Replace(ScriptsConstants.TeaPieFolderPathReference, _pathProvider.TeaPieFolderPath);
}
