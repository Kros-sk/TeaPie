namespace TeaPie.Testing;

internal interface ITestManager
{
    public void RegisterTest(Test test);

    public void RegisterTheory(Test test);

    public TestCaseTestingSummary GetSummary();
}
