namespace TeaPie;

internal static class Constants
{
    public const string RequestFileExtension = ".http";
    public const string ScriptFileExtension = ".csx";
    public const string NuGetPackageFileExtension = ".nupkg";
    public const string LibraryFileExtension = ".dll";

    public const string DefaultNuGetPackagesFolderName = "packages";
    public const string DefaultNuGetLibraryFolderName = "lib";

    public const string PreRequestSuffix = "-init";
    public const string RequestSuffix = "-req";
    public const string PostResponseSuffix = "-test";

    public const string NuGetApiResourcesUrl = "https://api.nuget.org/v3/index.json";

    public const string ApplicationName = "TeaPie";

    public static readonly IEnumerable<string> DefaultImports = [
        "System",
        "System.Collections.Generic",
        "System.IO",
        "System.Linq",
        "System.Net.Http",
        "System.Threading",
        "System.Threading.Tasks",
        "Microsoft.Extensions.Logging"
    ];

    public static readonly string[] CompatibleFrameworks =
    [
        "net8.0",
        "net7.0",
        "net6.0",
        "net5.0",
        "netcoreapp3.1",
        "netstandard2.1",
        "netstandard2.0"
    ];
}
