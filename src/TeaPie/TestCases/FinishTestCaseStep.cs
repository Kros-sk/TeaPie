using TeaPie.Pipelines;

namespace TeaPie.TestCases;

internal class FinishTestCaseStep : IPipelineStep
{
    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        context.CurrentTestCase = null;
        await Task.CompletedTask;
    }
}
