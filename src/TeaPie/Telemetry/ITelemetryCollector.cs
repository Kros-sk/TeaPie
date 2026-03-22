namespace TeaPie.Telemetry;

/// <summary>
/// Collects and provides access to telemetry data during a collection run.
/// </summary>
internal interface ITelemetryCollector
{
    /// <summary>
    /// Whether telemetry collection is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// The telemetry configuration options.
    /// </summary>
    TelemetryOptions Options { get; }

    /// <summary>
    /// Enables telemetry collection with optional configuration.
    /// </summary>
    /// <param name="options">The telemetry options.</param>
    void Enable(TelemetryOptions? options = null);

    /// <summary>
    /// Records an HTTP request telemetry entry.
    /// </summary>
    /// <param name="telemetry">The telemetry data for the request.</param>
    void RecordRequest(HttpRequestTelemetry telemetry);

    /// <summary>
    /// Gets the aggregated telemetry data.
    /// </summary>
    TelemetryData GetTelemetryData();
}
