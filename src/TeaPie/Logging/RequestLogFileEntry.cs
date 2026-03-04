using System.Text.Json.Serialization;

namespace TeaPie.Logging;

internal class RequestLogFileEntry
{
    public string RequestId { get; init; } = Guid.NewGuid().ToString();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double DurationMs => EndTime.Subtract(StartTime).TotalMilliseconds;
    public RequestInfo Request { get; set; } = new();
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseInfo? Response { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AuthInfo? Authentication { get; set; }
    public List<string> Errors { get; set; } = [];
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

internal class AuthInfo
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ProviderType { get; set; }
    public bool IsDefault { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? AuthenticatedAt { get; set; }
}
