namespace TeaPie.Testing;

public static class TeaPieTestingExtensions
{
    /// <summary>
    /// Executes the specified <paramref name="testFunction"/> as a test method. If <paramref name="testFunction"/>
    /// throws an exception, the test is considered <b>failed</b>. If no exception is thrown, the test is considered
    /// <b>passed</b>. The test will be referenced by <paramref name="testName"/> in the results.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="testName">The name of the test.</param>
    /// <param name="testFunction">The testing function to execute.</param>
    public static void Test(this TeaPie teaPie, string testName, Action testFunction)
        => teaPie._tester.Test(testName, testFunction);

    /// <summary>
    /// Executes the specified asynchronous <paramref name="testFunction"/> as a test method. If <paramref name="testFunction"/>
    /// throws an exception, the test is considered <b>failed</b>. If no exception is thrown, the test is considered
    /// <b>passed</b>. The test will be referenced by <paramref name="testName"/> in the results.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="testName">The name of the test.</param>
    /// <param name="testFunction">The asynchronous testing function to execute.</param>
    public static async Task Test(this TeaPie teaPie, string testName, Func<Task> testFunction)
        => await teaPie._tester.Test(testName, testFunction);
}
