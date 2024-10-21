namespace TeaPie.Parsing;

internal static class ParsingConstants
{
    public const string ReferenceScriptDirective = "#load";
    public const string NugetDirective = "#nuget";

    public const string LoadDirectivePattern = @"^#load\s+""([a-zA-Z0-9_\-\.\s\\\/]+\.([a-zA-Z0-9]+))""$";
    public const string NugetDirectivePattern = @"^#nuget\s+""([a-zA-Z0-9_.-]+),\s*([0-9]+\.[0-9]+\.[0-9]+)""$";
}
