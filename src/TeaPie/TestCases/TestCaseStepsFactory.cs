using Microsoft.Extensions.DependencyInjection;
using TeaPie.Http;
using TeaPie.Pipelines;

namespace TeaPie.TestCases;

internal static class TestCaseStepsFactory
{
    public static IPipelineStep[] CreateStepsForRequestsWithinTestCase(
      IServiceProvider serviceProvider,
      TestCaseExecutionContext testCaseExecutionContext)
        => CreateStepsForRequest(serviceProvider, testCaseExecutionContext, CreateStepsForRequestsWithinTestCase);

    public static IPipelineStep[] CreateStepsForTestsCase(
      IServiceProvider serviceProvider,
      TestCaseExecutionContext testCaseExecutionContext)
        => CreateStepsForRequest(serviceProvider, testCaseExecutionContext, CreateStepsForTestsCase);

    private static IPipelineStep[] CreateStepsForRequest(
        IServiceProvider serviceProvider,
        TestCaseExecutionContext testCaseExecutionContext,
        Func<IServiceProvider, IPipelineStep[]> createFunction)
    {
        using var scope = serviceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var accessor = provider.GetRequiredService<ITestCaseExecutionContextAccessor>();
        accessor.Context = testCaseExecutionContext;

        return createFunction(provider);
    }

    private static IPipelineStep[] CreateStepsForRequestsWithinTestCase(IServiceProvider provider)
        => [provider.GetStep<ReadHttpFileStep>(),
            provider.GetStep<GenerateStepsForRequestsStep>()];

    private static IPipelineStep[] CreateStepsForTestsCase(IServiceProvider provider)
        => [provider.GetStep<InitializeTestCaseStep>(),
            provider.GetStep<FinishTestCaseStep>()];
}
