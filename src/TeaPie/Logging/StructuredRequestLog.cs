using System.Text.Json.Serialization;

namespace TeaPie.Logging;

internal class StructuredRequestLog
{
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? DurationMs => EndTime?.Subtract(StartTime).TotalMilliseconds;
    public RequestInfo Request { get; set; } = new();
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseInfo? Response { get; set; }
    public RetryInfo Retries { get; set; } = new();
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AuthInfo? Authentication { get; set; }
    public List<string> Errors { get; set; } = [];
    public Dictionary<string, object> Metadata { get; set; } = [];
}

internal class RequestInfo
{
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Uri { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = [];
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Body { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentType { get; set; }
    public string FilePath { get; set; } = string.Empty;
}

internal class ResponseInfo
{
    public int StatusCode { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReasonPhrase { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Body { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContentType { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}

internal class RetryInfo
{
    public int AttemptCount { get; set; } = 1;
    public List<RetryAttempt> Attempts { get; set; } = [];
}

internal class RetryAttempt
{
    public int AttemptNumber { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Reason { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseInfo? Response { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Exception? Exception { get; set; }
    public bool IsSuccessful { get; set; }
    public double DurationMs { get; set; }
}

internal class AuthInfo
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ProviderType { get; set; }
    public bool IsDefault { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? AuthenticatedAt { get; set; }
}
