using Microsoft.Extensions.Logging;

namespace TeaPie.Logging;

internal class NuGetLoggerAdapter(ILogger<NuGetLoggerAdapter> logger) : NuGet.Common.ILogger
{
    private readonly ILogger<NuGetLoggerAdapter> _logger = logger;

    public void Log(NuGet.Common.LogLevel level, string data) => _logger.Log(level.ToMicrosoftLogLevel(), data);
    public void Log(NuGet.Common.ILogMessage message)
    {
        var logLevel = message.Level.ToMicrosoftLogLevel();
        _logger.Log(logLevel, "{NuGetMessage}", message.Message.Trim());
    }

    public async Task LogAsync(NuGet.Common.LogLevel level, string data) => await Task.Run(() => Log(level, data));
    public async Task LogAsync(NuGet.Common.ILogMessage message) => await Task.Run(() => Log(message));

    public void LogDebug(string data) => _logger.LogDebug(data);
    public void LogVerbose(string data) => _logger.LogTrace(data);
    public void LogInformation(string data) => _logger.LogInformation(data);
    public void LogMinimal(string data) => _logger.LogInformation(data);
    public void LogWarning(string data) => _logger.LogWarning(data);
    public void LogError(string data) => _logger.LogError(data);
    public void LogSummary(string data) => _logger.LogInformation(data);
    public void LogInformationSummary(string data) => _logger.LogInformation(data);
}
