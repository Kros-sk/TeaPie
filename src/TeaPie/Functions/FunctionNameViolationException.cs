namespace TeaPie.Functions;

internal class FunctionNameViolationException : Exception
{
    public FunctionNameViolationException() { }

    public FunctionNameViolationException(string? message) : base(message) { }

    public FunctionNameViolationException(string? message, Exception? innerException) : base(message, innerException) { }
}
