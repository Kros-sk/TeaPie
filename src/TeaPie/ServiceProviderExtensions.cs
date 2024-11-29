using Microsoft.Extensions.DependencyInjection;
using TeaPie.Pipelines;

namespace TeaPie;

internal static class ServiceProviderExtensions
{
    public static TStep GetStep<TStep>(this IServiceProvider provider) where TStep : IPipelineStep
        => provider.GetRequiredService<TStep>();
}
