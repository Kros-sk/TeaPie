using Serilog.Events;

namespace TeaPie.Logging;

internal sealed class TreeScope : IDisposable
{
    private readonly TreeScopeStateStore.ScopeState _state;
    private bool _disposed;

    public TreeScope()
    {
        _state = new TreeScopeStateStore.ScopeState();
        TreeScopeStateStore.Push(_state);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        TreeScopeStateStore.Pop(_state);

        if (_state.Printed)
        {
            TreeConsoleWriter.WriteClosing(_state.Depth, DateTimeOffset.Now, TreeConsoleWriter.LevelToShort(LogEventLevel.Information));
        }

        _disposed = true;
    }
}