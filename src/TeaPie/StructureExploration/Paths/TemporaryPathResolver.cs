namespace TeaPie.StructureExploration.Paths;

internal class TemporaryPathResolver(IPathProvider pathProvider, RelativePathResolver relativePathResolver) : IPathResolver
{
    private readonly IPathProvider _pathProvider = pathProvider;
    private readonly IPathResolver _relativePathResolver = relativePathResolver;

    public bool CanResolve(string path) => Directory.Exists(_pathProvider.TempFolderPath);

    /// <summary>
    /// Resolves temporary path for given <paramref name="path"/>. Parameter <paramref name="basePath"/> is used only if the path
    /// is relative, to root it. Root path and temporary path are provided by the <see cref="PathProvider"/>.
    /// </summary>
    /// <param name="path">Path from which temporary path should be computed.</param>
    /// <param name="basePath">If the <paramref name="path"/> is relative, this is used for rooting it.</param>
    /// <returns>Temporary path for specified path.</returns>
    public string ResolvePath(string path, string basePath)
    {
        if (_relativePathResolver.CanResolve(path))
        {
            path = _relativePathResolver.ResolvePath(path, basePath);
        }

        var relativePath = GetRelativePath(path);

        return Path.Combine(_pathProvider.TempFolderPath, relativePath);
    }

    private string GetRelativePath(string path)
    {
        if (path.Contains(_pathProvider.TeaPieFolderPath))
        {
            var relativePath = path.Replace(_pathProvider.TeaPieFolderPath, string.Empty).TrimSlashes();
            return Path.Combine(Constants.TeaPieFolderName, relativePath);
        }
        else if (path.Contains(_pathProvider.RootPath))
        {
            return path.Replace(_pathProvider.RootPath, _pathProvider.TempFolderPath);
        }
        else
        {
            var root = Path.GetPathRoot(path) ?? string.Empty;
            return path.Replace(root, string.Empty);
        }
    }
}
