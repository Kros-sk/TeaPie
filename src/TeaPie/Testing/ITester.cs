namespace TeaPie.Testing;

internal interface ITester
{
    void Test(string testName, Action testFunction, bool skipTest = false);

    Task Test(string testName, Func<Task> testFunction, bool skipTest = false);
}
