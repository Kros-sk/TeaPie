namespace TeaPie.Variables;

internal class VariableNameViolationException : Exception
{
    public VariableNameViolationException() { }

    public VariableNameViolationException(string? message) : base(message) { }

    public VariableNameViolationException(string? message, Exception? innerException) : base(message, innerException) { }
}
