namespace TeaPie.Logging;

internal static class TreeScopeStateStore
{
    internal sealed class ScopeState
    {
        public int Depth { get; set; }
        public bool Printed { get; set; }
    }

    private static readonly AsyncLocal<List<ScopeState>?> _current = new();

    internal static IReadOnlyList<ScopeState>? GetStack() => _current.Value;

    internal static void MarkPrinted(ScopeState state)
    {
        state.Printed = true;
    }

    internal static void Push(ScopeState state)
    {
        var list = _current.Value;
        if (list == null)
        {
            list = [];
            _current.Value = list;
        }

        list.Add(state);
        state.Depth = list.Count;
    }

    internal static void Pop(ScopeState state)
    {
        var list = _current.Value;
        if (list == null || list.Count == 0)
        {
            return;
        }

        if (list[^1] == state)
        {
            list.RemoveAt(list.Count - 1);
        }
        else
        {
            list.Remove(state);
        }

        if (list.Count == 0)
        {
            _current.Value = null;
        }
    }
}
