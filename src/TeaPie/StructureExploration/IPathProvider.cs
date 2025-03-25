namespace TeaPie.StructureExploration;

internal interface IPathProvider
{
    public string RootPath { get; }

    public string TempRootPath { get; }

    public string TeaPieFolderPath { get; }

    public string StructureName { get; }

    public string CacheFolderPath { get; }

    public string TempFolderPath { get; }

    public string ReportsFolderPath { get; }

    public string RunsFolderPath { get; }

    public string NuGetPackagesFolderPath { get; }

    public string VariablesFolderPath { get; }

    public string VariablesFilePath { get; }

    public void UpdatePaths(ApplicationContext applicationContext);

    public string ComputeTemporaryPath(string referencedPath, string directoryPath);

    public string ComputeRealPath(string tempPath, string directoryPath);
}
