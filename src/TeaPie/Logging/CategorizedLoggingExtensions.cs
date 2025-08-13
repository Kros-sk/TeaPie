using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace TeaPie.Logging;

internal static class CategorizedLoggingExtensions
{
    public static void LogTrace<T>(this ILogger<T> logger, LogCategory category, string? message, params object?[] args)
    {
        using (LogContext.PushProperty("Category", category.ToString()))
        {
            logger.LogTrace(message, args);
        }
    }

    public static void Log<T>(this ILogger<T> logger, LogLevel logLevel, LogCategory category, string? message, params object?[] args)
    {
        using (LogContext.PushProperty("Category", category.ToString()))
        {
            logger.Log(logLevel, message, args);
        }
    }
}
