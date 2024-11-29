using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Http;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using File = TeaPie.StructureExploration.File;

namespace TeaPie.Pipelines;

internal sealed class GenerateStepsForTestCasesStep(IPipeline pipeline) : IPipelineStep
{
    private readonly IPipeline _pipeline = pipeline;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        List<IPipelineStep> newSteps = [];
        foreach (var testCase in context.TestCases.Values)
        {
            foreach (var preReqScript in testCase.PreRequestScripts)
            {
                AddStepsForScript(context, preReqScript, newSteps);
            }

            AddStepsForRequest(context, testCase.Request, newSteps);

            foreach (var postResScript in testCase.PostResponseScripts)
            {
                AddStepsForScript(context, postResScript, newSteps);
            }
        }

        _pipeline.InsertSteps(this, [.. newSteps]);

        context.Logger.LogDebug("Multiple steps for all test cases ({Count}) were scheduled in the pipeline.",
            context.TestCases.Count);

        await Task.CompletedTask;
    }

    private static void AddStepsForRequest(ApplicationContext context, File request, List<IPipelineStep> newSteps)
    {
        var requestExecutionContext = new RequestExecutionContext(request);

        using var scope = context.ServiceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var accessor = provider.GetRequiredService<IRequestExecutionContextAccessor>();
        accessor.RequestExecutionContext = requestExecutionContext;

        newSteps.Add(provider.GetStep<ReadRequestFileStep>());
        newSteps.Add(provider.GetStep<ParseRequestFileStep>());
        newSteps.Add(provider.GetStep<ExecuteRequestStep>());
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
}
