using Microsoft.Extensions.Logging;
using TeaPie.Logging.Tree;
using TeaPie.Pipelines;
using TeaPie.StructureExploration;

namespace TeaPie.TestCases;

internal class GenerateStepsForTestCasesStep(IPipeline pipeline) : IPipelineStep
{
    private readonly IPipeline _pipeline = pipeline;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        List<IPipelineStep> newSteps = [];

        foreach (var collectionGroup in context.TestCases.GroupBy(tc => tc.ParentFolder.Path))
        {
            IDisposable? collectionScope = null;
            var collectionName = collectionGroup.First().ParentFolder.Name;

            newSteps.Add(new InlineStep((ctx, _) =>
            {
                collectionScope = ctx.Logger.BeginOuterTreeScope();
                ctx.Logger.LogInformation("Test Collection '{CollectionName}'", collectionName);
                return Task.CompletedTask;
            }));

            foreach (var testCase in collectionGroup)
            {
                AddStepsForTestCase(context, testCase, newSteps);
            }

            newSteps.Add(new InlineStep((ctx, _) =>
            {
                collectionScope?.Dispose();
                collectionScope = null;
                return Task.CompletedTask;
            }));
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
