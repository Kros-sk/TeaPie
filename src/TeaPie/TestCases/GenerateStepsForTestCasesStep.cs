using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;
using TeaPie.StructureExploration;

namespace TeaPie.TestCases;

internal class GenerateStepsForTestCasesStep(IPipeline pipeline) : IPipelineStep
{
    private readonly IPipeline _pipeline = pipeline;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        List<IPipelineStep> newSteps = [];
        foreach (var testCase in context.TestCases)
        {
            AddStepsForTestCase(context, testCase, newSteps);
        }

        _pipeline.InsertSteps(this, [.. newSteps]);

        context.Logger.LogDebug("Initialization steps for all test cases ({Count}) were scheduled in the pipeline.",
            context.TestCases.Count);

        await Task.CompletedTask;
    }

    private static void AddStepsForTestCase(
        ApplicationContext context, TestCase testCase, List<IPipelineStep> newSteps)
    {
        var testCaseExecutionContext = new TestCaseExecutionContext(testCase);
        newSteps.AddRange(TestCaseStepsFactory.CreateStepsForTestsCase(context.ServiceProvider, testCaseExecutionContext));
    }
}
