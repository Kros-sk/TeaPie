using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.TestCases;

internal static class Setup
{
    public static IServiceCollection AddTestCases(this IServiceCollection services)
    {
        services.AddScoped<ITestCaseExecutionContextAccessor, TestCaseExecutionContextAccessor>();
        services.AddSingleton<ICurrentTestCaseExecutionContextAccessor, CurrentTestCaseExecutionContextAccessor>();
        return services;
    }
}
