using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.Testing;

internal static class Setup
{
    public static IServiceCollection AddTesting(this IServiceCollection services)
    {
        services.AddSingleton<ITester, Tester>();
        services.AddSingleton<ITestResultsSummaryAccessor, TestResultsSummaryAccessor>();
        services.AddSingleton<ITestFactory, TestFactory>();
        services.AddSingleton<ITestScheduler, TestScheduler>();
        return services;
    }
}
