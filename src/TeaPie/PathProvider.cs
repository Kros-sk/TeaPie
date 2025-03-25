namespace TeaPie;

internal static class PathProvider
{
    public const string CacheFolderName = "cache";
    public const string RunsFolderName = "runs";
    public const string ReportsFolderName = "reports";

    public const string VariablesFolderName = "variables";
    public const string VariablesFileExtension = ".json";
    public const string VariablesFileNameWithoutExtension = "variables";
    public const string VariablesFileName = VariablesFileNameWithoutExtension + VariablesFileExtension;

    public const string TempFolderName = "temp";

    public static string GetHashedFolderName(string path)
    {
        var structureName = Path.GetFileNameWithoutExtension(path).TrimSuffix(Constants.RequestSuffix);
        return $"{structureName}-{path.GetHashCode()}";
    }

    public static string GetRelativePathToCacheFolder(string path)
        => Path.Combine(CacheFolderName, GetHashedFolderName(path));

    public static string GetRelativePathToVariablesFolder(string path)
        => Path.Combine(GetRelativePathToCacheFolder(path), VariablesFolderName);

    public static string GetRelativePathToVariablesFile(string path)
        => Path.Combine(GetRelativePathToVariablesFolder(path), VariablesFileName);

    public static string GetRelativePathToRunsFolder(string path)
        => Path.Combine(GetRelativePathToCacheFolder(path), RunsFolderName);

    public static string GetRelativePathToTempFolder(string path)
        => Path.Combine(TempFolderName, GetHashedFolderName(path));

    public static string GetRelativeReportsToTempFolder(string path)
        => Path.Combine(ReportsFolderName, GetHashedFolderName(path));
}
