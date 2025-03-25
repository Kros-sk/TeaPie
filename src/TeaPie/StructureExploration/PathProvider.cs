using TeaPie.Scripts;

namespace TeaPie.StructureExploration;

internal class PathProvider : IPathProvider
{
    private const string CacheFolderName = "cache";
    private const string ReportsFolderName = "reports";
    private const string TempFolderName = "temp";
    private const string NuGetPackagesFolderName = "packages";

    private const string VariablesFolderName = "variables";
    private const string RunsFolderName = "runs";

    private const string VariablesFileExtension = ".json";
    private const string VariablesFileNameWithoutExtension = "variables";
    private const string VariablesFileName = VariablesFileNameWithoutExtension + VariablesFileExtension;

    public string RootPath { get; private set; } = string.Empty;
    public string TempRootPath { get; private set; } = string.Empty;
    public string TeaPieFolderPath { get; private set; } = string.Empty;

    public string StructureName => Path.GetFileNameWithoutExtension(RootPath).TrimSuffix(Constants.RequestSuffix);

    public string CacheFolderPath => Path.Combine(TeaPieFolderPath, CacheFolderName, GetStructurePathHash());
    public string TempFolderPath => Path.Combine(TeaPieFolderPath, TempFolderName, GetStructurePathHash());
    public string ReportsFolderPath => Path.Combine(TeaPieFolderPath, ReportsFolderName, GetStructurePathHash());
    public string NuGetPackagesFolderPath => Path.Combine(TeaPieFolderPath, NuGetPackagesFolderName);

    public string RunsFolderPath => Path.Combine(CacheFolderPath, RunsFolderName);
    public string VariablesFolderPath => Path.Combine(CacheFolderPath, VariablesFolderName);
    public string VariablesFilePath => Path.Combine(VariablesFolderPath, VariablesFileName);

    private string GetStructurePathHash() => $"{StructureName}-{RootPath.GetHashCode()}";

    public void UpdatePaths(ApplicationContext applicationContext)
    {
        RootPath = applicationContext.Path;
        TempRootPath = applicationContext.TempFolderPath;
        TeaPieFolderPath = string.IsNullOrEmpty(applicationContext.TeaPieFolderPath)
            ? TempFolderPath
            : applicationContext.TeaPieFolderPath;
    }

    public string ComputeTemporaryPath(string referencedPath, string directoryPath)
    {
        if (referencedPath.StartsWith(ScriptsConstants.TeaPieFolderPathReference))
        {
            var resolvedPath = referencedPath.Replace(ScriptsConstants.TeaPieFolderPathReference, Constants.TeaPieFolderName);
            return Path.Combine(TempFolderPath, resolvedPath);
        }
        else if (Path.IsPathRooted(referencedPath))
        {
            var trimmedPath = referencedPath.TrimRootPath(RootPath);
            return Path.Combine(TempFolderPath, trimmedPath);
        }
        else
        {
            var absolutePath = Path.GetFullPath(Path.Combine(directoryPath, referencedPath));
            var trimmedPath = absolutePath.TrimRootPath(RootPath);
            return Path.Combine(TempFolderPath, trimmedPath);
        }
    }

    public string ComputeRealPath(string tempPath, string directoryPath)
    {
        var tempTeaPieFolderPath = Path.Combine(TempFolderPath, Constants.TeaPieFolderName);

        if (tempPath.StartsWith(tempTeaPieFolderPath))
        {
            return tempPath.Replace(tempTeaPieFolderPath, TeaPieFolderPath);
        }
        else
        {
            var trimmedPath = tempPath.TrimRootPath(TempFolderPath);
            var absolutePath = Path.Combine(RootPath, trimmedPath);

            return Path.IsPathRooted(absolutePath) ? absolutePath : Path.GetFullPath(Path.Combine(directoryPath, trimmedPath));
        }
    }
}
