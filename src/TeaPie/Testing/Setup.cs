using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.Testing;

internal static class Setup
{
    public static IServiceCollection AddTesting(this IServiceCollection services)
    {
        services.AddSingleton<ITester, Tester>();
        return services;
    }
}
