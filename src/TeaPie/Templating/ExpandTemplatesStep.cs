using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;
using TeaPie.TestCases;

namespace TeaPie.Templating;

internal sealed class ExpandTemplatesStep(
    ITestCaseExecutionContextAccessor testCaseExecutionContextAccessor,
    ITemplateExpander templateExpander,
    IExpandedContentDumper expandedContentDumper) : IPipelineStep
{
    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ExecutionContextValidator.Validate(
            testCaseExecutionContextAccessor, out TestCaseExecutionContext testCaseExecutionContext, "expand templates");
        ExecutionContextValidator.ValidateParameter(
            testCaseExecutionContext.RequestsFileContent, out string content, "expand templates", "the requests file's content");

        var expansion = templateExpander.Expand(content, testCaseExecutionContext.TestCase.RequestsFile.RelativePath);
        var expandedContent = expansion.Content;
        testCaseExecutionContext.RequestsFileContent = expandedContent;

        if (!TemplateDetector.ContainsTemplateTag(content))
        {
            return;
        }

        testCaseExecutionContext.TemplatedRequestNames = expansion.RequestNames;

        LogExpansionResult(context, testCaseExecutionContext, content, expandedContent);
        LogExpandedContent(context, testCaseExecutionContext, expandedContent);

        if (context.DumpExpandedContent)
        {
            expandedContentDumper.Dump(context, testCaseExecutionContext.TestCase, expandedContent);
        }

        await Task.CompletedTask;
    }

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
