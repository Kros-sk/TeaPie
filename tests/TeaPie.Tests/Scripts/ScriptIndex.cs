namespace TeaPie.Tests.Scripts;

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

    public static readonly string ScriptWithDuplicatedNuGetDirectivePath =
        Path.Combine(RootSubFolderPath, $"scriptWithDuplicatedNuGetDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithNonExistingScriptLoadDirectivePath =
        Path.Combine(RootSubFolderPath, $"scriptWithNonExistingScriptLoadDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithOneLoadDirectivePath =
        Path.Combine(RootSubFolderPath, $"scriptWithOneLoadDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithMultipleLoadDirectives =
        Path.Combine(RootSubFolderPath, $"scriptWithMultipleLoadDirectives{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithInvalidNuGetDirectivePath =
        Path.Combine(RootSubFolderPath, $"scriptWithInvalidNuGetDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithOneNuGetDirectivePath =
        Path.Combine(RootSubFolderPath, $"scriptWithOneNuGetDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithMultipleNuGetDirectivesPath =
        Path.Combine(RootSubFolderPath, $"scriptWithMultipleNuGetDirectives{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithMultipleLoadAndNuGetDirectivesPath =
        Path.Combine(RootSubFolderPath, $"scriptWithMultipleLoadAndNuGetDirectives{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithSyntaxErrorPath =
    Path.Combine(RootSubFolderPath, $"scriptWithSyntaxError{Constants.ScriptFileExtension}");
}
