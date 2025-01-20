namespace TeaPie;

public static class PathExtensions
{
    internal static string NormalizeSeparators(this string path)
        => path.Replace('\\', Path.DirectorySeparatorChar)
               .Replace('/', Path.DirectorySeparatorChar);

    internal static string Root(this string path)
        => Path.IsPathRooted(path) ? path : Path.GetFullPath(path);

    internal static string RemoveSlashAtTheEnd(this string path)
        => path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    internal static string TrimQuotes(this string path)
        => path.Trim('\"');

    public static string NormalizePath(this string path, bool rootPath = false)
        => rootPath
            ? path.NormalizePath().Root()
            : path.NormalizePath();

    private static string NormalizePath(this string path)
        => path.Trim().TrimQuotes().NormalizeSeparators().RemoveSlashAtTheEnd();
}
