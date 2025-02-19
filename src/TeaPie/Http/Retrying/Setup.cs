using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.Http.Retrying;

internal static class Setup
{
    public static IServiceCollection AddRetrying(this IServiceCollection services)
    {
        services.AddSingleton<IRetryStrategyRegistry, RetryStrategyRegistry>();
        services.AddSingleton<IResiliencePipelineProvider, ResiliencePipelineProvider>();

        return services;
    }
}
