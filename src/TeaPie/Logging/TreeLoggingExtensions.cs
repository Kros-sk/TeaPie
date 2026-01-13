using Microsoft.Extensions.Logging;

namespace TeaPie.Logging;

public static class TreeLoggingExtensions
{
    private static bool _treeLoggingEnabled = false;
    public static void SetTreeLoggingEnabled(bool enabled)
    {
        _treeLoggingEnabled = enabled;
    }

    public static IDisposable BeginTreeScope(this ILogger logger, bool? treeLoggingEnabled = null)
    {
        var enabled = treeLoggingEnabled ?? _treeLoggingEnabled;
        if (!enabled)
        {
            return EmptyDisposable.Instance;
        }
        return new TreeScope(logger, enabled);
    }
    private sealed class EmptyDisposable : IDisposable
    {
        public static readonly EmptyDisposable Instance = new();
        public void Dispose() { }
    }
}
