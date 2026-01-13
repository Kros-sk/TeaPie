using Microsoft.Extensions.Logging;

namespace TeaPie.Logging;

public static class TreeLoggingExtensions
{
    public static IDisposable BeginTreeScope(this ILogger logger, string name)
        => new TreeScope(logger, name);
}
