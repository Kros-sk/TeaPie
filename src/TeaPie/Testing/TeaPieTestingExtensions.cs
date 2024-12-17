namespace TeaPie.Testing;

public static class TeaPieTestingExtensions
{
    public static void Test(this TeaPie teaPie, string testName, Action testFunction)
        => teaPie._tester.Test(testName, testFunction);

    public static async Task Test(this TeaPie teaPie, string testName, Func<Task> testFunction)
        => await teaPie._tester.Test(testName, testFunction);
}
