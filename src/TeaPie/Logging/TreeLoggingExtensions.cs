using Microsoft.Extensions.Logging;

namespace TeaPie.Logging;

internal static class TreeLoggingExtensions
{
    private static bool _treeLoggingEnabled = false;
    public static void SetTreeLoggingEnabled(bool enabled)
    {
        _treeLoggingEnabled = enabled;
    }

    public static IDisposable BeginTreeScope(this ILogger logger)
    {
        _ = logger;
        if (!_treeLoggingEnabled)
        {
            return EmptyDisposable.Instance;
        }
        return new TreeScope();
    }
    private sealed class EmptyDisposable : IDisposable
    {
        public static readonly EmptyDisposable Instance = new();
        public void Dispose() { }
    }
}
