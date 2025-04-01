namespace TeaPie;

internal class ApplicationContextOptions(
    string? tempPath = null,
    string? environment = null,
    string? environmentFilePath = null,
    string? reportFilePath = null,
    string? initializationScriptPath = null,
    bool cacheVariables = true)
{
    public string TempFolderPath { get; set; } = tempPath ?? string.Empty;
    public string Environment { get; set; } = environment ?? string.Empty;
    public string EnvironmentFilePath { get; set; } = environmentFilePath ?? string.Empty;
    public string ReportFilePath { get; set; } = reportFilePath ?? string.Empty;
    public string InitializationScriptPath { get; set; } = initializationScriptPath ?? string.Empty;
    public bool CacheVariables { get; set; } = cacheVariables;
}
