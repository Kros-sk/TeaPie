using System.Collections.Immutable;
using Serilog.Events;

namespace TeaPie.Logging;

internal sealed class TreeScope : IDisposable
{
    private static readonly AsyncLocal<ImmutableStack<TreeScope>> _current = new();

    internal int Depth { get; }
    internal LogEventLevel? PrintedLevel { get; set; }
    internal bool Printed => PrintedLevel.HasValue;
    private bool _disposed;

    public TreeScope()
    {
        var stack = _current.Value ?? ImmutableStack<TreeScope>.Empty;
        Depth = stack.Count() + 1;
        _current.Value = stack.Push(this);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_current.Value?.IsEmpty == false)
        {
            _current.Value = _current.Value.Pop();
        }

        if (Printed)
        {
            TreeConsoleWriter.WriteClosing(Depth, DateTimeOffset.Now, TreeConsoleWriter.LevelToShort(PrintedLevel!.Value));
        }

        _disposed = true;
    }

    internal static IReadOnlyList<TreeScope>? GetActiveScopes()
    {
        var stack = _current.Value;
        if (stack?.IsEmpty != false)
        {
            return null;
        }

        return stack.Reverse().ToList();
    }

    internal static void MarkPrinted(TreeScope scope, LogEventLevel level) => scope.PrintedLevel = level;
}