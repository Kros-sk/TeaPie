namespace TeaPie.Testing;

internal record Test(string Name, Func<Task> Function)
{
    public TestResult Result { get; set; } = new TestResult.NotRun();
}
