using Microsoft.Extensions.Logging;

namespace TeaPie.Telemetry;

/// <summary>
/// Default in-memory telemetry collector that stores telemetry data for the duration of the run.
/// </summary>
internal class InMemoryTelemetryCollector(ILogger<InMemoryTelemetryCollector> logger) : ITelemetryCollector
{
    private readonly TelemetryData _data = new();
    private readonly ILogger<InMemoryTelemetryCollector> _logger = logger;
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
        NotifyCustomProvider(telemetry);
    }

    public TelemetryData GetTelemetryData() => _data;

    private void NotifyCustomProvider(HttpRequestTelemetry telemetry)
    {
        if (_options.CustomProvider is null)
        {
            return;
        }

        try
        {
            _options.CustomProvider.OnRequestRecordedAsync(telemetry).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Custom telemetry provider failed to process request telemetry.");
        }
    }
}
