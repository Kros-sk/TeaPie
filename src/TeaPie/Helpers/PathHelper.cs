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

    public static string MergeWith(this string path1, string path2)
    {
        var path1Segments = path1.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var path2Segments = path2.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        var overlapIndex = -1;
        var minLength = Math.Min(path1Segments.Length, path2Segments.Length);

        for (var i = 0; i < minLength; i++)
        {
            if (path1Segments[path1Segments.Length - 1 - i] == path2Segments[i])
            {
                overlapIndex = i;
            }
            else
            {
                break;
            }
        }

        if (overlapIndex >= 0)
        {
            var mergedSegments = path1Segments.Concat(path2Segments.Skip(overlapIndex + 1));
            return string.Join(Path.DirectorySeparatorChar, mergedSegments);
        }

        return Path.Combine(path1, path2);
    }
}
