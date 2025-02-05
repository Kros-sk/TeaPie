namespace TeaPie.Testing;

public class TestCaseTestResultsSummary(string name) : TestResultsSummary
{
    public string Name { get; } = name;
}
