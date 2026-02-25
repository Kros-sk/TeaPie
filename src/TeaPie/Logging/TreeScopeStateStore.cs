using System.Collections.Immutable;
using Serilog.Events;

namespace TeaPie.Logging;

internal static class TreeScopeStateStore
{
    internal sealed class ScopeState
    {
        public int Depth { get; set; }
        public LogEventLevel? PrintedLevel { get; set; }
        public bool Printed => PrintedLevel.HasValue;
    }

    private static readonly AsyncLocal<ImmutableStack<ScopeState>> _current = new();

    internal static IReadOnlyList<ScopeState>? GetActiveScopes()
    {
        var stack = _current.Value;
        if (stack?.IsEmpty != false)
        {
            return null;
        }

        return stack.Reverse().ToList();
    }

    internal static void MarkPrinted(ScopeState state, LogEventLevel level) => state.PrintedLevel = level;

    internal static void Push(ScopeState state)
    {
        var stack = _current.Value ?? ImmutableStack<ScopeState>.Empty;
        state.Depth = stack.Count() + 1;
        _current.Value = stack.Push(state);
    }

    internal static void Pop()
    {
        var stack = _current.Value;
        if (stack?.IsEmpty != false)
        {
            return;
        }

        _current.Value = stack.Pop();
    }
}
