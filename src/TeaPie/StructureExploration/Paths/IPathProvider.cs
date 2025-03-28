namespace TeaPie.StructureExploration.Paths;

internal interface IPathProvider
{
    string RootPath { get; }

    string TempRootPath { get; }

    string TeaPieFolderPath { get; }

    string StructureName { get; }

    string CacheFolderPath { get; }

    string TempFolderPath { get; }

    string ReportsFolderPath { get; }

    string RunsFolderPath { get; }

    string NuGetPackagesFolderPath { get; }

    string VariablesFolderPath { get; }

    string VariablesFilePath { get; }

    void UpdatePaths(string rootPath, string tempRootPath, string teaPieFolderPath = "");
}
