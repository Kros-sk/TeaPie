using Microsoft.Extensions.Logging;
using TeaPie.Logging;
using TeaPie.Pipelines;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
using File = System.IO.File;
using Timer = TeaPie.Logging.Timer;

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
            await Timer.Execute(
                async () => await File.ReadAllTextAsync(testCase.RequestsFile.Path, cancellationToken),
                elapsedTime => LogEndOfReading(context, testCase, elapsedTime));
    }

    private static void LogEndOfReading(ApplicationContext context, TestCase testCase, long elapsedTime)
        => context.Logger.LogTrace("Content of the requests file at path '{RequestPath}' was read in {Time}.",
            testCase.RequestsFile.RelativePath,
            elapsedTime.ToHumanReadableTime());

    private void ValidateContext(out TestCaseExecutionContext testCaseExecutionContext, out TestCase testCase)
    {
        ExecutionContextValidator.Validate(_testCaseContextAccessor, out testCaseExecutionContext, "read HTTP file");
        testCase = testCaseExecutionContext.TestCase;
    }
}
