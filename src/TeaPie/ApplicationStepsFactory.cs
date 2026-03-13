using TeaPie.Environments;
using TeaPie.Logging;
using TeaPie.Logging.Tree;
using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
using TeaPie.Variables;

namespace TeaPie;

internal static class ApplicationStepsFactory
{
    public static IPipelineStep[] CreateDefaultPipelineSteps(IServiceProvider provider)
    {
        IDisposable? initScope = null;

        return [
            new InlineStep((context, _) =>
            {
                initScope = context.Logger.BeginOuterTreeScope();
                return Task.CompletedTask;
            }),
            provider.GetStep<ResolvePathsStep>(),
            provider.GetStep<ExploreStructureStep>(),
            provider.GetStep<TryLoadVariablesStep>(),
            provider.GetStep<InitializeEnvironmentsStep>(),
            provider.GetStep<InitializeApplicationStep>(),
            new InlineStep((_, _) =>
            {
                initScope?.Dispose();
                initScope = null;
                return Task.CompletedTask;
            }),
            provider.GetStep<GenerateStepsForTestCasesStep>(),
            provider.GetStep<ReportTestResultsSummaryStep>(),
            provider.GetStep<SaveVariablesStep>()
        ];
    }

    public static IPipelineStep[] CreateStructureExplorationSteps(IServiceProvider provider)
        => [provider.GetStep<ResolvePathsStep>(),
            provider.GetStep<ExploreStructureStep>(),
            provider.GetStep<DisplayStructureStep>()];

    public static IPipelineStep[] CreateScriptCompilationSteps(IServiceProvider provider)
        => [provider.GetStep<ResolvePathsStep>(),
            provider.GetStep<InitializeApplicationStep>(),
            provider.GetStep<ReadScriptFileStep>(),
            provider.GetStep<PreProcessScriptStep>(),
            provider.GetStep<CompileScriptStep>()];
}
