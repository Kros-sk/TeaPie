using Microsoft.Extensions.Logging;
using Spectre.Console;
using TeaPie.Pipelines;
using TeaPie.TestCases;

namespace TeaPie.Templating;

internal sealed class ExpandTemplatesStep(
    ITestCaseExecutionContextAccessor testCaseExecutionContextAccessor,
    ITemplateExpander templateExpander) : IPipelineStep
{
    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ExecutionContextValidator.Validate(
            testCaseExecutionContextAccessor, out TestCaseExecutionContext testCaseExecutionContext, "expand templates");
        ExecutionContextValidator.ValidateParameter(
            testCaseExecutionContext.RequestsFileContent, out string content, "expand templates", "the requests file's content");

        var expandedContent = templateExpander.Expand(content, testCaseExecutionContext.TestCase.RequestsFile.RelativePath);
        testCaseExecutionContext.RequestsFileContent = expandedContent;

        if (!TemplateDetector.ContainsTemplateTag(content))
        {
            return;
        }

        testCaseExecutionContext.AllowsDuplicateRequestNames = true;

        LogExpansionResult(context, testCaseExecutionContext, content, expandedContent);
        LogExpandedContent(context, testCaseExecutionContext, expandedContent);

        if (context.DumpExpandedContent)
        {
            DumpExpandedContent(context, testCaseExecutionContext.TestCase.RequestsFile.RelativePath, expandedContent);
        }

        await Task.CompletedTask;
    }

    private static void DumpExpandedContent(ApplicationContext context, string relativePath, string expandedContent)
    {
        if (string.IsNullOrEmpty(context.ExpandedContentDumpPath))
        {
            AnsiConsole.Write(new Rule($"[grey]Expanded: {relativePath.EscapeMarkup()}[/]").LeftJustified());
            AnsiConsole.Write(new Text(expandedContent));
            AnsiConsole.WriteLine();
            return;
        }

        var dumpFilePath = Path.Combine(context.ExpandedContentDumpPath, ToRequestFilePath(relativePath));
        EnsureOutsideOfTestedCollection(context.Path, dumpFilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(dumpFilePath)!);
        System.IO.File.WriteAllText(dumpFilePath, expandedContent);

        context.Logger.LogInformation(
            "Expanded content of the requests file at '{Path}' was written to '{DumpFilePath}'.",
            relativePath,
            dumpFilePath);
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

    private static void LogExpansionResult(
        ApplicationContext context, TestCaseExecutionContext testCaseExecutionContext, string originalContent,
        string expandedContent)
        => context.Logger.LogDebug(
            "Templates within the requests file at '{Path}' were expanded (content length {OriginalLength} -> " +
            "{ExpandedLength}, {Change}).",
            testCaseExecutionContext.TestCase.RequestsFile.RelativePath,
            originalContent.Length,
            expandedContent.Length,
            string.Equals(originalContent, expandedContent, StringComparison.Ordinal)
                ? "unchanged"
                : "template(s) expanded");

    private static void LogExpandedContent(
        ApplicationContext context, TestCaseExecutionContext testCaseExecutionContext, string expandedContent)
        => context.Logger.LogTrace(
            "Requests file at '{Path}' after template expansion:{NewLine}{Content}",
            testCaseExecutionContext.TestCase.RequestsFile.RelativePath,
            Environment.NewLine,
            expandedContent);
}
