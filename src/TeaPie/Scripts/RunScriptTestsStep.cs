using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.TestCases;
using TeaPie.Testing;

namespace TeaPie.Scripts;

internal class RunScriptTestsStep(
    ITestCaseExecutionContextAccessor accessor,
    ITestResultsSummaryReporter resultsSummaryReporter,
    ITester testExecutor) : IPipelineStep
{
    private readonly ITestCaseExecutionContextAccessor _accessor = accessor;
    private readonly ITestResultsSummaryReporter _resultsSummaryReporter = resultsSummaryReporter;
    private readonly ITester _testExecutor = testExecutor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        _resultsSummaryReporter.Initialize();

        var testCaseExecutionContext = _accessor.Context!;
        foreach (var test in testCaseExecutionContext.GetTests())
        {
            await _testExecutor.ExecuteOrSkipTest(test, testCaseExecutionContext.TestCase);
        }
    }
}
