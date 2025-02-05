namespace TeaPie.Testing;

internal interface ITestResultsSummaryAccessor
{
    public TestResultsSummary? Summary { get; set; }
}

internal class TestResultsSummaryAccessor : ITestResultsSummaryAccessor
{
    public TestResultsSummary? Summary { get; set; }
}
