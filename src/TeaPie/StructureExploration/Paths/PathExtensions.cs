namespace TeaPie.StructureExploration.Paths;

public static class PathExtensions
{
    public static string TrimRootPath(this string fullPath, string rootPath, bool keepRootFolder = false)
    {
        var normalizedFullPath = Path.GetFullPath(fullPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedRootPath = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (normalizedFullPath.StartsWith(normalizedRootPath, StringComparison.OrdinalIgnoreCase))
        {
            if (keepRootFolder)
            {
                return normalizedFullPath[(normalizedRootPath.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
            }
            else
            {
                return Path.GetRelativePath(normalizedRootPath, normalizedFullPath);
            }
        }

        return fullPath;
    }

    internal static string NormalizeSeparators(this string path)
        => path.Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);

    internal static string Root(this string path)
        => Path.IsPathRooted(path) ? path : Path.GetFullPath(path);

    internal static string TrimSlashes(this string path)
        => path.TrimSlashAtTheEnd().TrimSlashInTheBeginning();

    internal static string TrimSlashAtTheEnd(this string path)
        => path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    internal static string TrimSlashInTheBeginning(this string path)
        => path.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    internal static string TrimQuotes(this string path)
        => path.Trim('\"');

    public static string NormalizePath(this string path, bool rootPath = false)
        => rootPath
            ? path.NormalizePath().Root()
            : path.NormalizePath();

    private static string NormalizePath(this string path)
        => path.Trim().TrimQuotes().NormalizeSeparators().TrimSlashAtTheEnd();
}
