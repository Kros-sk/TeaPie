using Microsoft.Extensions.Logging;

namespace TeaPie.Telemetry;

/// <summary>
/// Extension methods for enabling and configuring telemetry on the TeaPie context.
/// </summary>
public static class TeaPieTelemetryExtensions
{
    /// <summary>
    /// Enables telemetry collection with default options.
    /// Telemetry will track HTTP request performance, retry statistics, and provide a performance summary.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    public static void EnableTelemetry(this TeaPie teaPie)
    {
        var collector = GetCollector(teaPie);
        collector.Enable();
        teaPie.Logger.LogInformation("Telemetry has been enabled with default options.");
    }

    /// <summary>
    /// Enables telemetry collection with custom options.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="configure">An action to configure telemetry options.</param>
    public static void EnableTelemetry(this TeaPie teaPie, Action<TelemetryOptions> configure)
    {
        var options = new TelemetryOptions();
        configure(options);

        var collector = GetCollector(teaPie);
        collector.Enable(options);
        teaPie.Logger.LogInformation("Telemetry has been enabled with custom options.");
    }

    private static ITelemetryCollector GetCollector(TeaPie teaPie)
        => (ITelemetryCollector)teaPie._serviceProvider.GetService(typeof(ITelemetryCollector))!;
}
