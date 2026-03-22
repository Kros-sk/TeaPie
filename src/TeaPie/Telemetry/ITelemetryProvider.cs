namespace TeaPie.Telemetry;

/// <summary>
/// Interface for custom telemetry providers that can receive telemetry data
/// for integration with external monitoring systems.
/// </summary>
public interface ITelemetryProvider
{
    /// <summary>
    /// Called when an HTTP request telemetry entry is recorded.
    /// </summary>
    /// <param name="telemetry">The HTTP request telemetry data.</param>
    Task OnRequestRecordedAsync(HttpRequestTelemetry telemetry);

    /// <summary>
    /// Called when the collection run completes with the final aggregated telemetry data.
    /// </summary>
    /// <param name="data">The aggregated telemetry data.</param>
    Task OnCollectionCompleteAsync(TelemetryData data);
}
