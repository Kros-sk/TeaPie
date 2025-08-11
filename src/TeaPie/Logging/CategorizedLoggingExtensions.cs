using Microsoft.Extensions.Logging;

namespace TeaPie.Logging;

internal static class CategorizedLoggingExtensions
{
    public static void LogWithCategory<T>(this ILogger<T> logger, LogLevel logLevel, LogCategory category, string? message, params object?[] args)
    {
        var categorizedMessage = $"[{category}] {message}";
        logger.Log(logLevel, categorizedMessage, args);
    }

    public static void LogTraceWithCategory<T>(this ILogger<T> logger, LogCategory category, string? message, params object?[] args)
    {
        var categorizedMessage = $"[{category}] {message}";
        logger.LogTrace(categorizedMessage, args);
    }

    public static void LogInformationWithCategory<T>(this ILogger<T> logger, LogCategory category, string? message, params object?[] args)
    {
        var categorizedMessage = $"[{category}] {message}";
        logger.LogInformation(categorizedMessage, args);
    }

    public static void LogWarningWithCategory<T>(this ILogger<T> logger, LogCategory category, string? message, params object?[] args)
    {
        var categorizedMessage = $"[{category}] {message}";
        logger.LogWarning(categorizedMessage, args);
    }

    public static void LogErrorWithCategory<T>(this ILogger<T> logger, LogCategory category, string? message, params object?[] args)
    {
        var categorizedMessage = $"[{category}] {message}";
        logger.LogError(categorizedMessage, args);
    }
}
