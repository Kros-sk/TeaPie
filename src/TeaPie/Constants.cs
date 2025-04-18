namespace TeaPie;

internal static class Constants
{
    public const string RequestFileExtension = ".http";
    public const string ScriptFileExtension = ".csx";

    public const string PreRequestSuffix = "-init";
    public const string RequestSuffix = "-req";
    public const string PostResponseSuffix = "-test";

    public const string DefaultEnvironmentFileName = "env";
    public const string EnvironmentFileExtension = ".json";
    public const string DefaultEnvironmentName = "$shared";

    public const string PascalCasePattern = "([A-Z][a-z]*|[a-z]+)";

    public const string ApplicationName = "TeaPie";

    public const string DefaultInitializationScriptName = "init";

    public static readonly string SystemTemporaryFolderPath = Path.Combine(Path.GetTempPath(), TeaPieFolderName);

    public const string TeaPieFolderName = ".teapie";

    public const string UnixEndOfLine = "\n";
    public const string WindowsEndOfLine = "\r\n";

    public const string SecretVariableTag = "secret";
    public const string NoCacheVariableTag = "no-cache";
}
