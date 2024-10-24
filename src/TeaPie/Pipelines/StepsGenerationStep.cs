using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Scripts;
using TeaPie.ScriptHandling;
using TeaPie.StructureExploration.Records;

namespace TeaPie.Pipelines;

internal sealed class StepsGenerationStep : IPipelineStep
{
    private readonly IPipeline _pipeline;
    private readonly IServiceProvider _serviceProvider;
    private StepsGenerationStep(IPipeline pipeline, IServiceProvider serviceProvider)
    {
        _pipeline = pipeline;
        _serviceProvider = serviceProvider;
    }

    public static StepsGenerationStep Create(IPipeline pipeline, IServiceProvider serviceProvider)
        => new(pipeline, serviceProvider);

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        List<IPipelineStep> newSteps = [];
        foreach (var testCase in context.TestCases.Values)
        {
            foreach (var preReqScript in testCase.PreRequestScripts)
            {
                AddStepsForScript(preReqScript, newSteps);
            }

            foreach (var postResScript in testCase.PostResponseScripts)
            {
                AddStepsForScript(postResScript, newSteps);
            }
        }

        _pipeline.InsertSteps(this, [.. newSteps]);

        await Task.CompletedTask;
    }

    private void AddStepsForScript(Script preReqScript, List<IPipelineStep> newSteps)
    {
        var scriptExecutionContext = new ScriptExecutionContext(preReqScript);
        newSteps.Add(ReadScriptStep.Create(scriptExecutionContext));
        newSteps.Add(
            PreProcessScriptStep.Create(_pipeline, scriptExecutionContext, _serviceProvider));
        newSteps.Add(SaveTempScriptStep.Create(scriptExecutionContext));
    }
}
