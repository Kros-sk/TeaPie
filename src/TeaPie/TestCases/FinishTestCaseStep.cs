using TeaPie.Pipelines;

namespace TeaPie.TestCases;

internal class FinishTestCaseStep : IPipelineStep
{
    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        _ = context.CurrentTestCase
            ?? throw new InvalidOperationException("Unable to finish null test case.");

        context.CurrentTestCase = null;
        await Task.CompletedTask;
    }
}
