namespace TeaPie.Testing;

internal class TestManager : ITestManager
{
    private readonly List<Theory> _theories = [];
    private readonly List<Test> _tests = [];

    public int NumberOfTheories => _theories.Count;
    public bool ShouldRetry => _tests.Count > 0;

    public void RegisterTest(Test test)
    {
        _tests.Add(test);
    }

    public void RegisterTheory(Test test)
    {
        _tests.Add(test);
    }

    public TestCaseTestingSummary GetSummary()
    {
        return new TestCaseTestingSummary
        {
            NumberOfTests = _tests.Count + _theories.Count,
            NumberOfSucceed = _tests.Count(t => t.Result.Success), // TODO: Add count of succeed theories
            NumberOfFailed = _tests.Count(t => !t.Result.Success), // TODO: Add count of failed theories
            NumberOfIgnored = _tests.Count(t => !t.Executed), // TODO: Add count of ignored theories
            FailuresReasons = _tests.Where(t => !t.Result.Success).ToDictionary(t => t.Name, t => t.Result.Message!)
            // TODO: Add reasons of failed theories
        };
    }
}
