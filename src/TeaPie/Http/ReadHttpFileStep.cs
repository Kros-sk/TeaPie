using Microsoft.Extensions.Logging;
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

        try
        {
            testCaseExecutionContext.RequestsFileContent =
                await File.ReadAllTextAsync(testCase.RequestsFile.Path, cancellationToken);

            context.Logger.LogTrace("Content of the requests file on path '{RequestPath}' was read.",
                testCase.RequestsFile.RelativePath);
        }
        catch (Exception ex)
        {
            context.Logger.LogError("Reading of the requests file on path '{Path}' failed, because of '{ErrorMessage}'.",
                testCase.RequestsFile.RelativePath,
                ex.Message);

            throw;
        }
    }

    private void ValidateContext(out TestCaseExecutionContext testCaseExecutionContext, out TestCase testCase)
    {
        testCaseExecutionContext = _testCaseContextAccessor.TestCaseExecutionContext
            ?? throw new InvalidOperationException("Unable to read file if test case's execution context is null.");

        testCase = testCaseExecutionContext.TestCase;
    }
}
