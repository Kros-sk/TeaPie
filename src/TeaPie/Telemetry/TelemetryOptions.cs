namespace TeaPie.Telemetry;

/// <summary>
/// Options for configuring telemetry behavior.
/// </summary>
public class TelemetryOptions
{
    /// <summary>
    /// Whether to display the performance summary in the console output. Default is <c>true</c>.
    /// </summary>
    public bool EnableConsoleOutput { get; set; } = true;

    /// <summary>
    /// Custom telemetry provider for integration with external monitoring systems.
    /// </summary>
    public ITelemetryProvider? CustomProvider { get; set; }

    /// <summary>
    /// Configures a custom telemetry provider.
    /// </summary>
    /// <param name="provider">The custom provider instance.</param>
    public void UseCustomProvider(ITelemetryProvider provider) => CustomProvider = provider;
}
