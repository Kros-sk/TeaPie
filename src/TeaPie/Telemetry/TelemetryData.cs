namespace TeaPie.Telemetry;

/// <summary>
/// Aggregated telemetry data for a collection run.
/// </summary>
public class TelemetryData
{
    private readonly List<HttpRequestTelemetry> _requests = [];

    /// <summary>
    /// All recorded HTTP request telemetry entries.
    /// </summary>
    public IReadOnlyList<HttpRequestTelemetry> Requests => _requests;

    /// <summary>
    /// Total number of HTTP requests executed.
    /// </summary>
    public int TotalRequests => _requests.Count;

    /// <summary>
    /// Number of successful HTTP requests (2xx status codes).
    /// </summary>
    public int SuccessfulRequests => _requests.Count(r => r.IsSuccess);

    /// <summary>
    /// Success rate as a percentage (0-100).
    /// </summary>
    public double SuccessRate => TotalRequests > 0 ? (double)SuccessfulRequests / TotalRequests * 100 : 0;

    /// <summary>
    /// Average request duration in milliseconds.
    /// </summary>
    public double AverageDurationMs => TotalRequests > 0 ? _requests.Average(r => r.DurationMs) : 0;

    /// <summary>
    /// Minimum request duration in milliseconds.
    /// </summary>
    public long MinDurationMs => TotalRequests > 0 ? _requests.Min(r => r.DurationMs) : 0;

    /// <summary>
    /// Maximum request duration in milliseconds.
    /// </summary>
    public long MaxDurationMs => TotalRequests > 0 ? _requests.Max(r => r.DurationMs) : 0;

    /// <summary>
    /// Total number of retry attempts across all requests.
    /// </summary>
    public int TotalRetryAttempts => _requests.Sum(r => r.RetryAttempts);

    /// <summary>
    /// Number of requests that required at least one retry.
    /// </summary>
    public int RequestsWithRetries => _requests.Count(r => r.RetryAttempts > 0);

    /// <summary>
    /// Retry success rate as a percentage (0-100).
    /// </summary>
    public double RetrySuccessRate => RequestsWithRetries > 0
        ? (double)_requests.Count(r => r.RetryAttempts > 0 && r.RetrySucceeded) / RequestsWithRetries * 100
        : 0;

    /// <summary>
    /// Percentage of requests that required retries.
    /// </summary>
    public double RetryPercentage => TotalRequests > 0
        ? (double)RequestsWithRetries / TotalRequests * 100
        : 0;

    /// <summary>
    /// Total duration of all requests in seconds.
    /// </summary>
    public double TotalDurationSeconds => TotalRequests > 0 ? _requests.Sum(r => r.DurationMs) / 1000.0 : 0;

    /// <summary>
    /// Requests per second throughput.
    /// </summary>
    public double Throughput => TotalDurationSeconds > 0 ? TotalRequests / TotalDurationSeconds : 0;

    /// <summary>
    /// The fastest request.
    /// </summary>
    public HttpRequestTelemetry? FastestRequest => TotalRequests > 0
        ? _requests.MinBy(r => r.DurationMs)
        : null;

    /// <summary>
    /// The slowest request.
    /// </summary>
    public HttpRequestTelemetry? SlowestRequest => TotalRequests > 0
        ? _requests.MaxBy(r => r.DurationMs)
        : null;

    /// <summary>
    /// The most retried request.
    /// </summary>
    public HttpRequestTelemetry? MostRetriedRequest => RequestsWithRetries > 0
        ? _requests.Where(r => r.RetryAttempts > 0).MaxBy(r => r.RetryAttempts)
        : null;

    /// <summary>
    /// Records a new HTTP request telemetry entry.
    /// </summary>
    internal void RecordRequest(HttpRequestTelemetry telemetry) => _requests.Add(telemetry);
}
