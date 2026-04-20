namespace TeaPie.Telemetry;

/// <summary>
/// Represents telemetry data for a single HTTP request execution.
/// </summary>
public class HttpRequestTelemetry
{
    /// <summary>
    /// The HTTP method used (GET, POST, etc.).
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// The request URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP status code of the response.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Duration of the request in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Whether the request was considered successful (2xx status code).
    /// </summary>
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

    /// <summary>
    /// Number of retry attempts made for this request.
    /// </summary>
    public int RetryAttempts { get; set; }

    /// <summary>
    /// Whether the request ultimately succeeded after retries.
    /// </summary>
    public bool RetrySucceeded { get; set; }

    /// <summary>
    /// The name of the request file.
    /// </summary>
    public string RequestName { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the request was executed.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
