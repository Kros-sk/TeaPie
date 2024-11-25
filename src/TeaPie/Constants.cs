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
        "Microsoft.Extensions.Logging",
        "TeaPie.Variables"
    ];

    public static readonly string[] FrameworksPriorityList =
    [
        "net8.0",
        "net7.0",
        "net6.0",
        "net5.0",
        "netstandard2.1",
        "netstandard2.0",
        "netcoreapp3.1",
        "netcoreapp3.0",
        "netcoreapp2.2",
        "netcoreapp2.1",
        "netcoreapp2.0",
        "netcoreapp1.1",
        "netcoreapp1.0",
        "net48",
        "net472",
        "net471",
        "net47",
        "net462",
        "net461",
        "net46",
        "net452",
        "net451",
        "net45",
        "net403",
        "net40",
        "net35",
        "net20",
        "xamarinios",
        "xamarinmac",
        "xamarinandroid",
        "uap10.1",
        "uap10.0",
        "unity",
        "netmf"
    ];
}
