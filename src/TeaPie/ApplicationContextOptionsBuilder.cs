namespace TeaPie;

internal class ApplicationContextOptionsBuilder
{
    private string? _tempFolderPath;
    private string _environment = string.Empty;
    private string _environmentFilePath = string.Empty;
    private string _reportFilePath = string.Empty;
    private string _initializationScriptPath = string.Empty;
    private bool _variablesCaching = true;

    public ApplicationContextOptionsBuilder SetTempFolderPath(string? tempPath)
    {
        _tempFolderPath = tempPath ?? string.Empty;
        return this;
    }

    public ApplicationContextOptionsBuilder SetEnvironment(string? environment)
    {
        _environment = environment ?? string.Empty;
        return this;
    }

    public ApplicationContextOptionsBuilder SetEnvironmentFilePath(string? environmentFilePath)
    {
        _environmentFilePath = environmentFilePath ?? string.Empty;
        return this;
    }

    public ApplicationContextOptionsBuilder SetReportFilePath(string? reportFilePath)
    {
        _reportFilePath = reportFilePath ?? string.Empty;
        return this;
    }

    public ApplicationContextOptionsBuilder SetInitializationScriptPath(string? initializationScriptPath)
    {
        _initializationScriptPath = initializationScriptPath ?? string.Empty;
        return this;
    }

    public ApplicationContextOptionsBuilder SetVariablesCaching(bool cacheVariables)
    {
        _variablesCaching = cacheVariables;
        return this;
    }

    public ApplicationContextOptions Build()
    {
        return new ApplicationContextOptions(
            _tempFolderPath ?? Constants.SystemTemporaryFolderPath,
            _environment,
            _environmentFilePath,
            _reportFilePath,
            _initializationScriptPath,
            _variablesCaching
        );
    }
}
