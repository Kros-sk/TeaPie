namespace TeaPie.Telemetry;

/// <summary>
/// Default in-memory telemetry collector that stores telemetry data for the duration of the run.
/// </summary>
internal class InMemoryTelemetryCollector : ITelemetryCollector
{
    private readonly TelemetryData _data = new();
    private TelemetryOptions _options = new();

    public bool IsEnabled { get; private set; }

    public TelemetryOptions Options => _options;

    public void Enable(TelemetryOptions? options = null)
    {
        _options = options ?? new TelemetryOptions();
        IsEnabled = true;
    }

    public void RecordRequest(HttpRequestTelemetry telemetry)
    {
        if (!IsEnabled)
        {
            return;
        }

        _data.RecordRequest(telemetry);
        _options.CustomProvider?.OnRequestRecordedAsync(telemetry);
    }

    public TelemetryData GetTelemetryData() => _data;
}
