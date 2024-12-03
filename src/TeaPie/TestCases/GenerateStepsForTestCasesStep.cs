using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;
using TeaPie.StructureExploration;

namespace TeaPie.TestCases;

internal sealed class GenerateStepsForTestCasesStep(IPipeline pipeline) : IPipelineStep
{
    private readonly IPipeline _pipeline = pipeline;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        List<IPipelineStep> newSteps = [];
        foreach (var testCase in context.TestCases.Values)
        {
            AddStepsForTestCase(context, testCase, newSteps);
        }

        _pipeline.InsertSteps(this, [.. newSteps]);

        context.Logger.LogDebug("Steps for all test cases ({Count}) were scheduled in the pipeline.",
            context.TestCases.Count);

        await Task.CompletedTask;
    }

    private static void AddStepsForTestCase(ApplicationContext context, TestCase testCase, List<IPipelineStep> newSteps)
    {
        var testCaseExecutionContext = new TestCaseExecutionContext(testCase);

        using var scope = context.ServiceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var accessor = provider.GetRequiredService<ITestCaseExecutionContextAccessor>();
        accessor.TestCaseExecutionContext = testCaseExecutionContext;

        newSteps.Add(provider.GetStep<InitializeTestCaseStep>());
        newSteps.Add(provider.GetStep<FinishTestCaseStep>());
    }
}
