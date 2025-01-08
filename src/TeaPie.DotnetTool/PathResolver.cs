namespace TeaPie.DotnetTool;

internal static class PathResolver
{
    public static string Resolve(string? path, string valueIfNull)
        => path is null ? valueIfNull : Resolve(path);

    public static string Resolve(string path)
        => Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
}
