namespace TeaPie.StructureExploration.Paths;

internal class RelativePathResolver : IPathResolver
{
    public bool CanResolve(string path) => !Path.IsPathRooted(path);

    public string ResolvePath(string path, string basePath)
        => Path.GetFullPath(Path.Combine(basePath, path));
}
