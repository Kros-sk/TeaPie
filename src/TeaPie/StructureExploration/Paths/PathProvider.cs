using System.Security.Cryptography;
using System.Text;

namespace TeaPie.StructureExploration.Paths;

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

    public string CacheFolderPath => Path.Combine(TeaPieFolderPath, CacheFolderName);
    public string TempFolderPath => Path.Combine(CacheFolderPath, TempFolderName, GetStructurePathHash());
    public string NuGetPackagesFolderPath => Path.Combine(CacheFolderPath, NuGetPackagesFolderName);
    public string ReportsFolderPath => Path.Combine(TeaPieFolderPath, ReportsFolderName);

    public string RunsFolderPath => Path.Combine(TeaPieFolderPath, RunsFolderName);
    public string VariablesFolderPath => Path.Combine(TeaPieFolderPath, CacheFolderName, VariablesFolderName);
    public string VariablesFilePath => Path.Combine(VariablesFolderPath, VariablesFileName);

    private string GetStructurePathHash() => $"{StructureName}-{GetHash(RootPath)}";

    public void UpdatePaths(string rootPath, string tempRootPath, string teaPieFolderPath = "")
    {
        RootPath = rootPath;
        TempRootPath = tempRootPath;
        TeaPieFolderPath = string.IsNullOrEmpty(teaPieFolderPath)
            ? TempRootPath
            : teaPieFolderPath;
    }

    private static readonly Dictionary<string, string> _hashes = [];

    private static string GetHash(string path)
    {
        if (!_hashes.TryGetValue(path, out var hash))
        {
            hash = GenerateHash(path);
            _hashes.Add(path, hash);
        }

        return hash;
    }

    private static string GenerateHash(string path)
    {
        var inputBytes = Encoding.UTF8.GetBytes(Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar).ToLowerInvariant());
        var hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
