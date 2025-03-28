namespace TeaPie.StructureExploration.Paths;

internal class PathResolver(IPathProvider pathProvider) : IPathResolver
{
    private readonly List<IPathResolver> _resolvers =
    [
        new TeaPieFolderPathResolver(pathProvider),
        new RelativePathResolver()
    ];

    public bool CanResolve(string path) => !string.IsNullOrEmpty(path);

    public string ResolvePath(string path, string basePath)
    {
        if (!CanResolve(path))
        {
            throw new InvalidOperationException($"Unable to resolve path '{path}'.");
        }

        foreach (var resolver in _resolvers)
        {
            if (resolver.CanResolve(path))
            {
                return resolver.ResolvePath(path, basePath);
            }
        }

        return path;
    }
}
