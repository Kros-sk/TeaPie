using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;

namespace TeaPie.Testing;

internal class ExecuteScheduledTestsStep(ITestScheduler scheduler, ITester tester) : IPipelineStep
{
    private readonly ITestScheduler _scheduler = scheduler;
    private readonly ITester _tester = tester;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        while (_scheduler.HasScheduledTest())
        {
            var test = _scheduler.Dequeue();
            await _tester.Test(test.Name, test.Function);
            context.Logger.LogDebug("Scheduled test with name '{TestName}' was executed.", test.Name);
        }
    }
}
