using Microsoft.Extensions.DependencyInjection;

namespace TeaPie.Telemetry;

internal static class Setup
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services)
        => services.AddSingleton<ITelemetryCollector, InMemoryTelemetryCollector>();
}
