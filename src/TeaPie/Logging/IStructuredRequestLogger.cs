using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeaPie.Logging;

internal interface IStructuredRequestLogger
{
    Task LogRequestAsync(StructuredRequestLog requestLog, CancellationToken cancellationToken = default);
}

internal class StructuredRequestLogger : IStructuredRequestLogger
{
    private readonly string? _outputDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public StructuredRequestLogger(string? outputDirectory = null)
    {
        _outputDirectory = outputDirectory;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task LogRequestAsync(StructuredRequestLog requestLog, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_outputDirectory))
        {
            return;
        }

        Directory.CreateDirectory(_outputDirectory);

        var fileName = $"request_{requestLog.RequestId}_{requestLog.StartTime:yyyyMMdd_HHmmss_fff}.json";
        var filePath = Path.Combine(_outputDirectory, fileName);

        var json = JsonSerializer.Serialize(requestLog, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }
}

internal class NullStructuredRequestLogger : IStructuredRequestLogger
{
    public Task LogRequestAsync(StructuredRequestLog requestLog, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
