using System.Collections.Immutable;

namespace TeaPie.Logging;

internal static class TreeScopeStateStore
{
    internal sealed class ScopeState
    {
        public int Depth { get; set; }
        public bool Printed { get; set; }
    }

    private static readonly AsyncLocal<ImmutableStack<ScopeState>> _current = new();

    internal static IReadOnlyList<ScopeState>? GetStack()
    {
        var stack = _current.Value;
        if (stack?.IsEmpty != false)
        {
            return null;
        }

        return stack.Reverse().ToList();
    }

    internal static void MarkPrinted(ScopeState state) => state.Printed = true;

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
