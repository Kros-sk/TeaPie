namespace TeaPie;

internal static class Constants
{
    public const string RequestFileExtension = ".http";
    public const string ScriptFileExtension = ".csx";
    public const string NugetPackageFileExtension = ".nupkg";

    public const string DefaultNugetPackageFolderName = "packages";

    public const string PreRequestSuffix = "-init";
    public const string RequestSuffix = "-req";
    public const string PostResponseSuffix = "-test";

    public const string ReferenceScriptDirective = "#load";
    public const string NugetDirectivePrefix = "#nuget";

    public const string NugetApiResourcesUrl = "https://api.nuget.org/v3/index.json";
}
