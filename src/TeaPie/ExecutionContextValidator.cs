using System.Diagnostics.CodeAnalysis;

namespace TeaPie;

internal static class ExecutionContextValidator
{
    public static void Validate<TAccessor, TContext>(TAccessor accessor, out TContext context, string activityName)
        where TAccessor : IContextAccessor<TContext>
        => context = accessor.Context
            ?? throw new InvalidOperationException($"Unable to {activityName} if execution context is null.");

    public static void ValidateParameter<TParameter>(
        TParameter parameter,
        [NotNull] out TParameter validated,
        string activityName,
        string parameterName)
        => validated = parameter
            ?? throw new InvalidOperationException($"Unable to {activityName} if {parameterName} is null.");
}
