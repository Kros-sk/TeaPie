using Microsoft.Extensions.Logging;
using TeaPie.Application;
using TeaPie.Pipelines;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
using File = System.IO.File;

namespace TeaPie.Http;

internal sealed class ReadHttpFileStep(ITestCaseExecutionContextAccessor testCaseExecutionContextAccessor) : IPipelineStep
{
    private readonly ITestCaseExecutionContextAccessor _testCaseContextAccessor = testCaseExecutionContextAccessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var testCaseExecutionContext, out var testCase);

        await ReadHttpFile(context, testCaseExecutionContext, testCase, cancellationToken);
    }

    private static async Task ReadHttpFile(
        ApplicationContext context,
        TestCaseExecutionContext testCaseExecutionContext,
        TestCase testCase,
        CancellationToken cancellationToken)
    {
        testCaseExecutionContext.RequestsFileContent =
            await File.ReadAllTextAsync(testCase.RequestsFile.Path, cancellationToken);

        context.Logger.LogTrace("Content of the requests file on path '{RequestPath}' was read.",
            testCase.RequestsFile.RelativePath);
    }

    private void ValidateContext(out TestCaseExecutionContext testCaseExecutionContext, out TestCase testCase)
    {
        ExecutionContextValidator.Validate(_testCaseContextAccessor, out testCaseExecutionContext, "read HTTP file");
        testCase = testCaseExecutionContext.TestCase;
    }
}
