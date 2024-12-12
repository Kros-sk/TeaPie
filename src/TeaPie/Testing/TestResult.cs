namespace TeaPie.Testing;

internal class TestResult(string name)
{
    public string TestName { get; } = name;
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? StackTrace { get; set; }
}
