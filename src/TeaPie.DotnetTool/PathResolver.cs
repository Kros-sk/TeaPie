namespace TeaPie.DotnetTool;

internal static class PathResolver
{
    public static string Resolve(string? path, string valueIfNull)
        => path is null ? valueIfNull : Resolve(path);

    public static string Resolve(string path)
        => Trim(path).NormalizeSeparators().Root();

    private static string Trim(string path)
    {
        path = path.Trim('\"');
        return path.Trim();
    }
}
