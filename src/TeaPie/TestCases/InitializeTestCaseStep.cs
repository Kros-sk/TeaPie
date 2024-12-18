using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Http;
using TeaPie.Pipelines;
using TeaPie.Scripts;
using Script = TeaPie.StructureExploration.Script;

namespace TeaPie.TestCases;

internal class InitializeTestCaseStep(ITestCaseExecutionContextAccessor accessor, IPipeline pipeline) : IPipelineStep
{
    private readonly IPipeline _pipeline = pipeline;
    private readonly ITestCaseExecutionContextAccessor _testCaseExecutionContextAccessor = accessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var testCaseExecutionContext);

        context.CurrentTestCase = testCaseExecutionContext;
        AddSteps(context, testCaseExecutionContext);

        await Task.CompletedTask;
    }

    private void AddSteps(ApplicationContext context, TestCaseExecutionContext testCaseExecutionContext)
    {
        List<IPipelineStep> newSteps = [];

        AddStepsForPreRequestScripts(context, testCaseExecutionContext, newSteps);
        AddStepsForRequests(context, testCaseExecutionContext, newSteps);
        AddStepsForPostResponseScripts(context, testCaseExecutionContext, newSteps);

        _pipeline.InsertSteps(this, [.. newSteps]);

        context.Logger.LogDebug("Multiple steps for all test cases ({Count}) were scheduled in the pipeline.",
            context.TestCases.Count);
    }

    private static void AddStepsForPreRequestScripts(
        ApplicationContext context,
        TestCaseExecutionContext testCaseExecutionContext,
        List<IPipelineStep> newSteps)
        => AddStepsForScripts(
            context,
            testCaseExecutionContext.TestCase.PreRequestScripts,
            testCaseExecutionContext.RegisterPreRequestScript,
            newSteps);

    private static void AddStepsForPostResponseScripts(
        ApplicationContext context,
        TestCaseExecutionContext testCaseExecutionContext,
        List<IPipelineStep> newSteps)
        => AddStepsForScripts(
            context,
            testCaseExecutionContext.TestCase.PostResponseScripts,
            testCaseExecutionContext.RegisterPostResponseScript,
            newSteps);

    private static void AddStepsForScripts(
        ApplicationContext context,
        IEnumerable<Script> scriptsCollection,
        Action<string, ScriptExecutionContext> addToCollection,
        List<IPipelineStep> newSteps)
    {
        foreach (var script in scriptsCollection)
        {
            addToCollection(script.File.Path, new(script));
            AddStepsForScript(context, script, newSteps);
        }
    }

    private static void AddStepsForScript(ApplicationContext context, Script script, List<IPipelineStep> newSteps)
    {
        var scriptExecutionContext = new ScriptExecutionContext(script);

        using var scope = context.ServiceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var accessor = provider.GetRequiredService<IScriptExecutionContextAccessor>();
        accessor.ScriptExecutionContext = scriptExecutionContext;

        newSteps.Add(provider.GetStep<ReadScriptFileStep>());
        newSteps.Add(provider.GetStep<PreProcessScriptStep>());
        newSteps.Add(provider.GetStep<SaveTempScriptStep>());
        newSteps.Add(provider.GetStep<CompileScriptStep>());
        newSteps.Add(provider.GetStep<ExecuteScriptStep>());
    }

    private static void AddStepsForRequests(
        ApplicationContext context,
        TestCaseExecutionContext testCaseExecutionContext,
        List<IPipelineStep> newSteps)
    {
        using var scope = context.ServiceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var accessor = provider.GetRequiredService<ITestCaseExecutionContextAccessor>();
        accessor.TestCaseExecutionContext = testCaseExecutionContext;

        newSteps.Add(provider.GetStep<ReadHttpFileStep>());
        newSteps.Add(provider.GetStep<GenerateStepsForRequestsStep>());
    }

    private void ValidateContext(out TestCaseExecutionContext testCaseExecutionContext)
    {
        testCaseExecutionContext = _testCaseExecutionContextAccessor.TestCaseExecutionContext
            ?? throw new InvalidOperationException("Unable to initialize test case if its execution context is null.");
    }
}
