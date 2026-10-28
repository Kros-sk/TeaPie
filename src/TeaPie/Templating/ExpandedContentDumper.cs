using Microsoft.Extensions.Logging;
using Spectre.Console;
using TeaPie.StructureExploration;

namespace TeaPie.Templating;

internal sealed class ExpandedContentDumper(IAnsiConsole console) : IExpandedContentDumper
{
    // Checked for every test case before the run, so that a wrong dump directory stops it before any request is sent,
    // not only once the first templated file is reached.
    public void EnsureDumpsAreOutsideOfTestedCollection(ApplicationContext context)
    {
        if (!context.DumpExpandedContent || string.IsNullOrEmpty(context.ExpandedContentDumpPath))
        {
            return;
        }

        foreach (var testCase in context.TestCases)
        {
            EnsureOutsideOfTestedCollection(context.Path, GetDumpFilePath(context, testCase));
        }
    }

    public void Dump(ApplicationContext context, TestCase testCase, string expandedContent)
    {
        var relativePath = testCase.RequestsFile.RelativePath;
        var tpTestCaseName = GetTpTestCaseName(testCase);
        var source = tpTestCaseName is null ? relativePath : $"{relativePath} ({tpTestCaseName})";

        if (string.IsNullOrEmpty(context.ExpandedContentDumpPath))
        {
            console.Write(new Rule($"[grey]Expanded: {source.EscapeMarkup()}[/]").LeftJustified());
            console.Write(new Text(expandedContent));
            console.WriteLine();
            return;
        }

        var dumpFilePath = GetDumpFilePath(context, testCase);
        EnsureOutsideOfTestedCollection(context.Path, dumpFilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(dumpFilePath)!);
        System.IO.File.WriteAllText(dumpFilePath, expandedContent);

        context.Logger.LogInformation(
            "Expanded content of the requests file at '{Path}' was written to '{DumpFilePath}'.",
            source,
            dumpFilePath);
    }

    // All test cases of a .tp file share its relative path, so a named one gets its name in the dump file name
    // to keep the dumps apart. An unnamed test case is named after the file, which needs no suffix.
    private static string? GetTpTestCaseName(TestCase testCase)
        => testCase.IsFromTpFile && !testCase.Name.Equals(
            Path.GetFileNameWithoutExtension(testCase.RequestsFile.RelativePath), StringComparison.OrdinalIgnoreCase)
            ? testCase.Name
            : null;

    private static string GetDumpFilePath(ApplicationContext context, TestCase testCase)
    {
        var relativePath = testCase.RequestsFile.RelativePath;
        var tpTestCaseName = GetTpTestCaseName(testCase);

        return Path.Combine(
            context.ExpandedContentDumpPath,
            ToRequestFilePath(
                tpTestCaseName is null ? relativePath : $"{relativePath}.{ToFileNamePart(tpTestCaseName)}"));
    }

    private static string ToFileNamePart(string name)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(character => invalidCharacters.Contains(character) ? '_' : character));
    }

    private static void EnsureOutsideOfTestedCollection(string testedPath, string dumpFilePath)
    {
        var collectionPath = Directory.Exists(testedPath) ? testedPath : Path.GetDirectoryName(testedPath)!;
        var collectionPrefix = Path.GetFullPath(collectionPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (Path.GetFullPath(dumpFilePath).StartsWith(collectionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Expanded content cannot be written to '{dumpFilePath}', because it is inside the tested collection " +
                $"'{collectionPath}' (it could overwrite a requests file or be run as an extra test case). Choose a " +
                "dump directory outside of the collection.");
        }
    }

    private static string ToRequestFilePath(string relativePath)
        => Path.GetExtension(relativePath).Equals(Constants.RequestFileExtension, StringComparison.OrdinalIgnoreCase)
            ? relativePath
            : relativePath + Constants.RequestFileExtension;
}
