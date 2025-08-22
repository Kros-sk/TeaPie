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

    public static void LogDebug<T>(this ILogger<T> logger, LogCategory category, string? message, params object?[] args)
    {
        using (LogContext.PushProperty("Category", category.ToString()))
        {
            logger.LogDebug(message, args);
        }
    }

    public static void LogDebug(this ILogger logger, LogCategory category, string? message, params object?[] args)
    {
        using (LogContext.PushProperty("Category", category.ToString()))
        {
            logger.LogDebug(message, args);
        }
    }

    public static void LogInformation<T>(this ILogger<T> logger, LogCategory category, string? message, params object?[] args)
    {
        using (LogContext.PushProperty("Category", category.ToString()))
        {
            logger.LogInformation(message, args);
        }
    }

    public static void LogWarning<T>(this ILogger<T> logger, LogCategory category, string? message, params object?[] args)
    {
        using (LogContext.PushProperty("Category", category.ToString()))
        {
            logger.LogWarning(message, args);
        }
    }

    public static void LogError<T>(this ILogger<T> logger, LogCategory category, string? message, params object?[] args)
    {
        using (LogContext.PushProperty("Category", category.ToString()))
        {
            logger.LogError(message, args);
        }
    }

    public static void LogCritical<T>(this ILogger<T> logger, LogCategory category, string? message, params object?[] args)
    {
        using (LogContext.PushProperty("Category", category.ToString()))
        {
            logger.LogCritical(message, args);
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
