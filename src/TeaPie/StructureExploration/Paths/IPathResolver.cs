namespace TeaPie.StructureExploration.Paths;

internal interface IPathResolver
{
    bool CanResolve(string path);

    string ResolvePath(string path, string basePath);
}
