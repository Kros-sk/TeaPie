using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace TeaPie.Logging;

internal static class TreeLoggingExtensions
{
    private static bool _treeLoggingEnabled;
    internal static void SetTreeLoggingEnabled(bool enabled)
    {
        _treeLoggingEnabled = enabled;
    }

    internal static IDisposable BeginTreeScope(this ILogger logger)
    {
        _ = logger;
        if (!_treeLoggingEnabled)
        {
            return EmptyDisposable.Instance;
        }
        return new TreeScope();
    }

    internal static IDisposable BeginOuterTreeScope(this ILogger logger,
        LogEventLevel level = LogEventLevel.Information)
    {
        _ = logger;
        if (!_treeLoggingEnabled)
        {
            return EmptyDisposable.Instance;
        }

        TreeScope.IncrementOuterDepth();
        TreeConsoleWriter.WriteOpening(TreeScope.OuterDepth, DateTimeOffset.Now,
            TreeConsoleFormatter.LevelToShort(level));
        return new OuterScopeDisposable(level);
    }

    private sealed class OuterScopeDisposable(LogEventLevel level) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            var depth = TreeScope.OuterDepth;
            TreeScope.DecrementOuterDepth();
            TreeConsoleWriter.WriteClosing(depth, DateTimeOffset.Now, TreeConsoleFormatter.LevelToShort(level));
        }
    }

    private sealed class EmptyDisposable : IDisposable
    {
        public static readonly EmptyDisposable Instance = new();
        public void Dispose() { }
    }
}
