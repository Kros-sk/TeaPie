namespace TeaPie;

internal class ApplicationContextOptions(
    string? tempPath,
    string? environment,
    string? environmentFilePath,
    string? reportFilePath,
    string? initializationScriptPath)
{
    public string TempFolderPath { get; set; } = tempPath ?? string.Empty;
    public string Environment { get; set; } = environment ?? string.Empty;
    public string EnvironmentFilePath { get; set; } = environmentFilePath ?? string.Empty;
    public string ReportFilePath { get; set; } = reportFilePath ?? string.Empty;
    public string InitializationScriptPath { get; set; } = initializationScriptPath ?? string.Empty;
}
