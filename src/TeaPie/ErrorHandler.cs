using Microsoft.Extensions.Logging;

namespace TeaPie;

internal static class ErrorHandler
{
    internal static void Handle(ApplicationContext appContext, Exception ex, Type stepType, ILogger logger)
    {
        var source = stepType.Name.SplitPascalCase();
        logger.LogError("Exception was thrown during execution of '{StepName}'. Error message: {Message}",
            source,
            ex.Message);

        appContext.PrematureTermination = new(source, TerminationType.ApplicationError, ex.Message, (int)ExitCode.GeneralError);

        logger.LogDebug("Stack trace: {StackTrace}", ex.StackTrace);
    }
}
