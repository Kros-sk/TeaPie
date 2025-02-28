namespace TeaPie.Scripts;

internal static class ScriptsConstants
{
    public const string DefaultNuGetPackagesFolderName = "packages";
    public const string DefaultNuGetLibraryFolderName = "lib";

    public const string NuGetPackageFileExtension = ".nupkg";

    public const string LibraryFileExtension = ".dll";

    public const string NuGetApiResourcesUrl = "https://api.nuget.org/v3/index.json";

    public static readonly HashSet<string> SuppressedWarnings =
    [
        "CS1701"
    ];

    public static readonly IEnumerable<NuGetPackageDescription> DefaultNuGetPackages =
    [
        new("Microsoft.CSharp", "4.7.0"),
        new("Xunit.Assert", "2.9.3")
    ];

    public static readonly IEnumerable<string> DefaultImports = [
        "Microsoft.Extensions.Logging",
        "Polly",
        "Polly.Retry",
        "System",
        "System.Collections.Generic",
        "System.IO",
        "System.Linq",
        "System.Net",
        "System.Net.Http",
        "System.Threading",
        "System.Threading.Tasks",
        "TeaPie",
        "TeaPie.Http",
        "TeaPie.Http.Auth",
        "TeaPie.Http.Auth.OAuth2",
        "TeaPie.Http.Parsing",
        "TeaPie.Http.Retrying",
        "TeaPie.Json",
        "TeaPie.Reporting",
        "TeaPie.TeaPie",
        "TeaPie.Testing",
        "TeaPie.Variables",
        "Xunit.Assert"
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
