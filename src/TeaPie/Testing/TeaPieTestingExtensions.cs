using TeaPie.Http.Parsing;

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
    /// <param name="skipTest">Indicates whether the test should be skipped (<see langword="true"/>) or
    /// normally executed (<see langword="false"/>). Defaults to <see langword="false"/>.</param>
    public static void Test(this TeaPie teaPie, string testName, Action testFunction, bool skipTest = false)
        => teaPie._tester.Test(testName, testFunction, skipTest);

    /// <summary>
    /// Executes the specified asynchronous <paramref name="testFunction"/> as a test method. If <paramref name="testFunction"/>
    /// throws an exception, the test is considered <b>failed</b>. If no exception is thrown, the test is considered
    /// <b>passed</b>. The test will be referenced by <paramref name="testName"/> in the results.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="testName">The name of the test.</param>
    /// <param name="testFunction">The asynchronous testing function to execute.</param>
    /// <param name="skipTest">Indicates whether the test should be skipped (<see langword="true"/>) or
    /// normally executed (<see langword="false"/>). Defaults to <see langword="false"/>.</param>
    public static async Task Test(this TeaPie teaPie, string testName, Func<Task> testFunction, bool skipTest = false)
        => await teaPie._tester.Test(testName, testFunction, skipTest);

    public static async Task RegisterTestDirective(
        this TeaPie teaPie,
        string directiveName,
        string directivePattern,
        Func<IReadOnlyDictionary<string, object>, string> testNameGetter,
        Func<HttpResponseMessage, IReadOnlyDictionary<string, object>, Task> testFunction)
    {
        var directive = new TestDirective(directiveName, directivePattern, testNameGetter, testFunction);

        TestDirectivesLineParser.RegisterTestDirective(directivePattern);
        teaPie._predefinedTestFactory.RegisterTestType(directiveName, testFunction);
    }
}
