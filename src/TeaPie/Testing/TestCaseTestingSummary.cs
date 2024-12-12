namespace TeaPie.Testing;

internal class TestCaseTestingSummary
{
    public int NumberOfTests { get; set; }
    public int NumberOfSucceed { get; set; }
    public int NumberOfFailed { get; set; }
    public int NumberOfIgnored { get; set; }
    public Dictionary<string, string> FailuresReasons { get; set; } = [];
}
