using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;
using TeaPie.TestCases;

namespace TeaPie.Http;

internal sealed class ReadHttpFileStep(ITestCaseExecutionContextAccessor testCaseExecutionContextAccessor) : IPipelineStep
{
    private readonly ITestCaseExecutionContextAccessor _testCaseContextAccessor = testCaseExecutionContextAccessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        var testCaseExecutionContext = _testCaseContextAccessor.TestCaseExecutionContext
            ?? throw new NullReferenceException("Test case's execution context is null.");

        var testCase = testCaseExecutionContext.TestCase;

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
}
