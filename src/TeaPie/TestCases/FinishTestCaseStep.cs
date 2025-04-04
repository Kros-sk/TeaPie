using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;

namespace TeaPie.TestCases;

internal class FinishTestCaseStep : IPipelineStep
{
    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        _ = context.CurrentTestCase ?? throw new InvalidOperationException(
            "Unable to finish test case if current test case's execution context is null.");

        LogEndOfTestCase(context);

        context.CurrentTestCase = null;
        await Task.CompletedTask;
    }

    private static void LogEndOfTestCase(ApplicationContext context)
        => context.Logger.LogInformation("Execution of test case '{Name}' has finished. ({Progress})",
            context.CurrentTestCase!.TestCase.Name,
            $"{context.CurrentTestCase.Id}/{context.TestCases.Count}");
}
