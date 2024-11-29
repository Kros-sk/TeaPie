namespace TeaPie.Tests.Scripts;

internal static class ScriptIndex
{
    public const string RootFolderName = "Demo";
    public const string RootSubFolderName = "Scripts";
    public static readonly string RootSubFolderRelativePath = Path.Combine(RootFolderName, RootSubFolderName);
    public static readonly string RootSubFolderFullPath = Path.Combine(Environment.CurrentDirectory, RootSubFolderRelativePath);
    public static readonly string RootFolderFullPath = Path.Combine(Environment.CurrentDirectory, RootFolderName);

    public static readonly string EmptyScriptPath =
        Path.Combine(RootSubFolderFullPath, $"emptyScript{Constants.ScriptFileExtension}");

    public static readonly string PlainScriptPath =
        Path.Combine(RootSubFolderFullPath, $"plainScript{Constants.ScriptFileExtension}");

    public static readonly string ScriptAccessingTeaPieLogger =
        Path.Combine(RootSubFolderFullPath, $"scriptAccessingTeaPieLogger{Constants.ScriptFileExtension}");

    public static readonly string ScriptManipulatingWithVariables =
        Path.Combine(RootSubFolderFullPath, $"scriptManipulatingWithVariables{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithDuplicatedNuGetDirectivePath =
        Path.Combine(RootSubFolderFullPath, $"scriptWithDuplicatedNuGetDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithNonExistingScriptLoadDirectivePath =
        Path.Combine(RootSubFolderFullPath, $"scriptWithNonExistingScriptLoadDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithOneLoadDirectivePath =
        Path.Combine(RootSubFolderFullPath, $"scriptWithOneLoadDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithMultipleLoadDirectives =
        Path.Combine(RootSubFolderFullPath, $"scriptWithMultipleLoadDirectives{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithInvalidNuGetDirectivePath =
        Path.Combine(RootSubFolderFullPath, $"scriptWithInvalidNuGetDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithOneNuGetDirectivePath =
        Path.Combine(RootSubFolderFullPath, $"scriptWithOneNuGetDirective{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithMultipleNuGetDirectivesPath =
        Path.Combine(RootSubFolderFullPath, $"scriptWithMultipleNuGetDirectives{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithMultipleLoadAndNuGetDirectivesPath =
        Path.Combine(RootSubFolderFullPath, $"scriptWithMultipleLoadAndNuGetDirectives{Constants.ScriptFileExtension}");

    public static readonly string ScriptWithSyntaxErrorPath =
    Path.Combine(RootSubFolderFullPath, $"scriptWithSyntaxError{Constants.ScriptFileExtension}");
}
