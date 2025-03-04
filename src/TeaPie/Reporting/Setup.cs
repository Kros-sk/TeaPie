using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.Reporting;

internal static class Setup
{
    public static IServiceCollection AddReporting(
        this IServiceCollection services, IApplicationAbstractFactory applicationAbstractFactory)
        => applicationAbstractFactory.AddTestResultsSummaryReporter(services);
}
