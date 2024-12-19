using TeaPie.Pipelines;
using TeaPie.StructureExploration;
using TeaPie.TestCases;

namespace TeaPie;

internal static class ApplicationStepsFactory
{
    public static IPipelineStep[] CreateDefaultPipelineSteps(IServiceProvider provider)
        => [provider.GetStep<ExploreStructureStep>(),
            provider.GetStep<PrepareTemporaryFolderStep>(),
            provider.GetStep<GenerateStepsForTestCasesStep>()];
}
