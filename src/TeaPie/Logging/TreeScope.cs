using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace TeaPie.Logging;

public sealed class TreeScope : IDisposable
{
    private static readonly AsyncLocal<int> _currentDepth = new();
    private readonly ILogger _logger;
    private readonly string _name;
    private readonly IDisposable? _logContextScope;
    private bool _disposed;

    public TreeScope(ILogger logger, string name)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _name = name ?? throw new ArgumentNullException(nameof(name));

        _currentDepth.Value++;
        _logContextScope = LogContext.PushProperty(ScopeDepthEnricher.ScopeDepthPropertyName, _currentDepth.Value);

        _logger.LogInformation("┌──", _name);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _logger.LogInformation("└──");
        _logContextScope?.Dispose();
        _currentDepth.Value--;
        _disposed = true;
    }

    internal static int CurrentDepth => _currentDepth.Value;
}

internal class ScopeState
{
    public int Depth { get; }
    public string Name { get; }

    public ScopeState(int depth, string name)
    {
        Depth = depth;
        Name = name;
    }

    public override string ToString() => Name;
}
