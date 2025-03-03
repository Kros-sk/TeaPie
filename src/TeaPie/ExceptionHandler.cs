using Microsoft.Extensions.Logging;

namespace TeaPie;

internal static class ExceptionHandler
{
    internal static void Handle(Exception ex, Type stepType, ILogger logger)
    {
        logger.LogError("Exception was thrown during execution of '{StepName}'. Error message: {Message}",
            stepType.Name.SplitPascalCase(),
            ex.Message);

        logger.LogDebug("Stack trace: {StackTrace}", ex.StackTrace);
    }
}
