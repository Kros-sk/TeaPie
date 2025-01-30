using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.Reporting;

internal static class Setup
{
    public static IServiceCollection AddReporting(this IServiceCollection services)
        => services.AddSingleton<ITestsResultsSummaryReporter, TestsResultsSummaryReporter>();
}
