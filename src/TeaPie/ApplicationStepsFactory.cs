using TeaPie.Environments;
using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.StructureExploration;
using TeaPie.TestCases;

namespace TeaPie;

internal static class ApplicationStepsFactory
{
    public static IPipelineStep[] CreateDefaultPipelineSteps(IServiceProvider provider)
        => [provider.GetStep<ExploreStructureStep>(),
            provider.GetStep<InitializeEnvironmentsStep>(),
            provider.GetStep<PrepareTemporaryFolderStep>(),
            provider.GetStep<InitializeApplicationStep>(),
            provider.GetStep<GenerateStepsForTestCasesStep>(),
            provider.GetStep<ReportTestResultsSummaryStep>()];

    public static IPipelineStep[] CreateStructureExplorationSteps(IServiceProvider provider)
        => [provider.GetStep<ExploreStructureStep>(),
            provider.GetStep<DisplayStructureStep>()];
}
