namespace TeaPie;

public static class PathExtensions
{
    public static string NormalizeSeparators(this string path)
        => path.Replace('\\', Path.DirectorySeparatorChar)
               .Replace('/', Path.DirectorySeparatorChar);

    public static string Root(this string path)
        => Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
}
