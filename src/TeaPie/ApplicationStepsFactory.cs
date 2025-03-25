using TeaPie.Environments;
using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
using TeaPie.Variables;

namespace TeaPie;

internal static class ApplicationStepsFactory
{
    public static IPipelineStep[] CreateDefaultPipelineSteps(IServiceProvider provider)
        => [provider.GetStep<ResolvePathsStep>(),
            provider.GetStep<ExploreStructureStep>(),
            provider.GetStep<InitializeEnvironmentsStep>(),
            provider.GetStep<TryLoadVariablesStep>(),
            provider.GetStep<InitializeApplicationStep>(),
            provider.GetStep<GenerateStepsForTestCasesStep>(),
            provider.GetStep<ReportTestResultsSummaryStep>(),
            provider.GetStep<SaveVariablesStep>()
        ];

    public static IPipelineStep[] CreateStructureExplorationSteps(IServiceProvider provider)
        => [provider.GetStep<ExploreStructureStep>(),
            provider.GetStep<DisplayStructureStep>()];
}
