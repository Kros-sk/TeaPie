namespace TeaPie;

internal static class Constants
{
    public const string RequestFileExtension = ".http";
    public const string ScriptFileExtension = ".csx";

    public const string PreRequestSuffix = "-init";
    public const string RequestSuffix = "-req";
    public const string PostResponseSuffix = "-test";

    public const string EnvironmentFileSuffix = "-env";
    public const string EnvironmentFileExtension = ".json";
    public const string DefaultEnvironmentName = "$shared";

    public const string PascalCasePattern = "([A-Z][a-z]*|[a-z]+)";

    public const string ApplicationName = "TeaPie";

    public static readonly string DefaultTemporaryFolderPath = Path.Combine(Path.GetTempPath(), ApplicationName);
}
