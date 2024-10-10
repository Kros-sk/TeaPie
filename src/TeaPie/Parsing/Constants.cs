namespace TeaPie.Utils;
internal class Constants
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

    public const string HttpGetMethodDirective = "GET";
    public const string HttpPutMethodDirective = "PUT";
    public const string HttpPostMethodDirective = "POST";
    public const string HttpPatchMethodDirective = "PATCH";
    public const string HttpDeleteMethodDirective = "DELETE";
}
