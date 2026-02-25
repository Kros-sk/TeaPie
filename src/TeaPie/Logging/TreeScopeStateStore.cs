using System.Collections.Immutable;
using Serilog.Events;

namespace TeaPie.Logging;

internal static class TreeScopeStateStore
{
    internal sealed class ScopeState
    {
        internal int Depth { get; set; }
        internal LogEventLevel? PrintedLevel { get; set; }
        internal bool Printed => PrintedLevel.HasValue;
    }

    private static readonly AsyncLocal<(ImmutableStack<ScopeState> Stack, int Depth)> _current = new();

    internal static IReadOnlyList<ScopeState>? GetActiveScopes()
    {
        var (stack, _) = _current.Value;
        if (stack?.IsEmpty != false)
        {
            return null;
        }

        return stack.Reverse().ToList();
    }

    internal static void MarkPrinted(ScopeState state, LogEventLevel level) => state.PrintedLevel = level;

    internal static void Push(ScopeState state)
    {
        var (stack, depth) = _current.Value;
        stack ??= [];
        state.Depth = depth + 1;
        _current.Value = (stack.Push(state), depth + 1);
    }

    internal static void Pop()
    {
        var (stack, depth) = _current.Value;
        if (stack?.IsEmpty != false)
        {
            return;
        }

        _current.Value = (stack.Pop(), depth - 1);
    }
}
