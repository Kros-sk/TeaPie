namespace TeaPie.Helpers;

internal static class PathHelper
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
}
