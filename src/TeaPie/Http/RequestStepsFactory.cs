using Microsoft.Extensions.DependencyInjection;
using TeaPie.Pipelines;

namespace TeaPie.Http;

internal static class RequestStepsFactory
{
    public static IPipelineStep[] CreateStepsForRequest(
        IServiceProvider serviceProvider,
        RequestExecutionContext requestExecutionContext)
    {
        using var scope = serviceProvider.CreateScope();
        var provider = scope.ServiceProvider;

        var accessor = provider.GetRequiredService<IRequestExecutionContextAccessor>();
        accessor.Context = requestExecutionContext;

        return CreateSteps(provider);
    }

    private static IPipelineStep[] CreateSteps(IServiceProvider provider)
        => [provider.GetStep<ParseHttpRequestStep>(),
            provider.GetStep<ExecuteRequestStep>(),
            provider.GetStep<DisposeRequestStep>()];
}
