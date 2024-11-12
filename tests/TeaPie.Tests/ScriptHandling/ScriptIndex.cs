namespace TeaPie.Tests.ScriptHandling;

internal static class ScriptIndex
{
    public const string RootFolderName = "Demo";
    public const string RootSubFolder = "Scripts";
    public static readonly string RootFolderPath = Path.Combine(Environment.CurrentDirectory, RootFolderName);

    public static readonly string RootSubFolderPath =
        Path.Combine(Environment.CurrentDirectory, RootFolderName, RootSubFolder);

    public static readonly string EmptyScriptPath =
        Path.Combine(RootSubFolderPath, $"emptyScript{Constants.ScriptFileExtension}");

    public static readonly string PlainScriptPath =
        Path.Combine(RootSubFolderPath, $"plainScript{Constants.ScriptFileExtension}");

    public static readonly string ScriptAccessingTeaPieInstance =
        Path.Combine(RootSubFolderPath, $"scriptAccessingTeaPieInstance{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithDuplicatedNugetDirectivePath =
        Path.Combine(RootSubFolderPath, $"scriptWithDuplicatedNugetDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithNonExistingScriptLoadDirectivePath =
        Path.Combine(RootSubFolderPath, $"scriptWithNonExistingScriptLoadDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithOneLoadDirectivePath =
        Path.Combine(RootSubFolderPath, $"scriptWithOneLoadDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithMultipleLoadDirectives =
        Path.Combine(RootSubFolderPath, $"scriptWithMultipleLoadDirectives{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithInvalidNugetDirectivePath =
        Path.Combine(RootSubFolderPath, $"scriptWithInvalidNugetDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithOneNugetDirectivePath =
        Path.Combine(RootSubFolderPath, $"scriptWithOneNugetDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithMultipleNugetDirectivesPath =
        Path.Combine(RootSubFolderPath, $"scriptWithMultipleNugetDirectives{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithMultipleLoadAndNugetDirectivesPath =
        Path.Combine(RootSubFolderPath, $"scriptWithMultipleLoadAndNugetDirectives{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithSyntaxErrorPath =
    Path.Combine(RootSubFolderPath, $"scriptWithSyntaxError{Constants.ScriptFileExtension}");
}
