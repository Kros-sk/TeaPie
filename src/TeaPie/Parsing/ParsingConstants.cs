namespace TeaPie.Parsing;

internal static class ParsingConstants
{
    public const string LoadScriptDirective = "#load";
    public const string NuGetDirective = "#nuget";

    public const string LoadDirectivePattern = @"^#load\s+""([a-zA-Z0-9_\-\.\s\\\/]+\.([a-zA-Z0-9]+))""$";
    public const string NuGetDirectivePattern = @"^#nuget\s+""([a-zA-Z0-9_.-]+),\s*([0-9]+\.[0-9]+\.[0-9]+)""$";
}
