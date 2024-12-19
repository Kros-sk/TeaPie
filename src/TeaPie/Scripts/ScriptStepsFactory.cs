using Microsoft.Extensions.DependencyInjection;
using TeaPie.Pipelines;

namespace TeaPie.Scripts;

internal static class ScriptStepsFactory
{
    public static IPipelineStep[] CreateStepsForScriptPreProcess(
        IServiceProvider serviceProvider,
        ScriptExecutionContext scriptExecutionContext)
        => CreateSteps(serviceProvider, scriptExecutionContext, GetStepsForScriptPreProcess);

    public static IEnumerable<IPipelineStep> CreateStepsForScriptPreProcessAndExecution(
        IServiceProvider serviceProvider,
        ScriptExecutionContext scriptExecutionContext)
        => CreateSteps(serviceProvider, scriptExecutionContext, GetStepsForScriptPreProcessAndExecution);

    private static IPipelineStep[] CreateSteps(
      IServiceProvider serviceProvider,
      ScriptExecutionContext scriptExecutionContext,
      Func<IServiceProvider, IPipelineStep[]> pipelines)
    {
        using var scope = serviceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var accessor = provider.GetRequiredService<IScriptExecutionContextAccessor>();
        accessor.Context = scriptExecutionContext;

        return pipelines(provider);
    }

    private static IPipelineStep[] GetStepsForScriptPreProcess(IServiceProvider provider)
        => [provider.GetStep<ReadScriptFileStep>(),
            provider.GetStep<PreProcessScriptStep>(),
            provider.GetStep<SaveTempScriptStep>()];

    private static IPipelineStep[] GetStepsForScriptPreProcessAndExecution(IServiceProvider provider)
        => [provider.GetStep<ReadScriptFileStep>(),
            provider.GetStep<PreProcessScriptStep>(),
            provider.GetStep<SaveTempScriptStep>(),
            provider.GetStep<CompileScriptStep>(),
            provider.GetStep<ExecuteScriptStep>()];
}
