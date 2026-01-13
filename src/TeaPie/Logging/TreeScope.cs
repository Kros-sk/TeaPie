using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace TeaPie.Logging;

public sealed class TreeScope : IDisposable
{
    private static readonly AsyncLocal<int> _currentDepth = new();
    private readonly ILogger _logger;
    private readonly IDisposable? _logContextScope;
    private readonly bool _treeLoggingEnabled;
    private bool _disposed;

    public TreeScope(ILogger logger, bool treeLoggingEnabled = true)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _treeLoggingEnabled = treeLoggingEnabled;

        _currentDepth.Value++;
        _logContextScope = LogContext.PushProperty(ScopeDepthEnricher.ScopeDepthPropertyName, _currentDepth.Value);

        if (_treeLoggingEnabled)
        {
            _logger.LogInformation("┌──");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_treeLoggingEnabled)
        {
            _logger.LogInformation("└──");
        }
        _logContextScope?.Dispose();
        _currentDepth.Value--;
        _disposed = true;
    }

    internal static int CurrentDepth => _currentDepth.Value;
}