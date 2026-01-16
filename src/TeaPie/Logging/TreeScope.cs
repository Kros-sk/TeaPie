using Microsoft.Extensions.Logging;

namespace TeaPie.Logging;

public sealed class TreeScope : IDisposable
{
    private readonly ILogger _logger;
    private readonly bool _treeLoggingEnabled;
    private readonly TreeScopeStateStore.ScopeState? _state;
    private bool _disposed;

    public TreeScope(ILogger logger, bool treeLoggingEnabled = true)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _treeLoggingEnabled = treeLoggingEnabled;

        if (_treeLoggingEnabled)
        {
            _state = new TreeScopeStateStore.ScopeState();
            TreeScopeStateStore.Push(_state);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_treeLoggingEnabled && _state != null)
        {
            TreeScopeStateStore.Pop(_state);

            if (_state.Printed)
            {
                TreeConsoleWriter.WriteClosing(_state.Depth);
            }
        }

        _disposed = true;
    }
}