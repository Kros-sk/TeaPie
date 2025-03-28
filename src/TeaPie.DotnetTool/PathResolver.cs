using TeaPie.StructureExploration.Paths;

namespace TeaPie.DotnetTool;

internal static class PathResolver
{
    public static string Resolve(string? path, string valueIfNull)
        => path is null ? valueIfNull : path.NormalizePath(true);
}
