using Microsoft.Extensions.DependencyInjection;
using TeaPie.Pipelines;
using TeaPie.TestCases;

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

    public static IEnumerable<IPipelineStep> CreateStepsForScriptExecution(
        IServiceProvider serviceProvider,
        TestCaseExecutionContext testCaseExecutionContext)
        => CreateSteps(serviceProvider, testCaseExecutionContext, GetStepsForScriptsExecution);

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

    private static IPipelineStep[] CreateSteps(
      IServiceProvider serviceProvider,
      TestCaseExecutionContext testCaseExecutionContext,
      Func<IServiceProvider, IPipelineStep[]> pipelines)
    {
        using var scope = serviceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var accessor = provider.GetRequiredService<ITestCaseExecutionContextAccessor>();
        accessor.Context = testCaseExecutionContext;

        return pipelines(provider);
    }

    private static IPipelineStep[] GetStepsForScriptPreProcess(IServiceProvider provider)
        => [provider.GetStep<ReadScriptFileStep>(),
            provider.GetStep<PreProcessScriptStep>(),
            provider.GetStep<SaveTempScriptStep>(),
            provider.GetStep<DisposeScriptStep>()];

    private static IPipelineStep[] GetStepsForScriptPreProcessAndExecution(IServiceProvider provider)
        => [provider.GetStep<ReadScriptFileStep>(),
            provider.GetStep<PreProcessScriptStep>(),
            provider.GetStep<SaveTempScriptStep>(),
            provider.GetStep<CompileScriptStep>(),
            provider.GetStep<ExecuteScriptStep>(),
            provider.GetStep<DisposeScriptStep>()];

    private static IPipelineStep[] GetStepsForScriptsExecution(IServiceProvider provider)
    => [provider.GetStep<RunScriptTestsStep>()];
}
