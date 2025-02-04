public void Initialize()
{
    // By default, environments are defined in a <collection-name>-env.json file.
    // Use the option '--env-file <path-to-environment-file>' to specify a custom environment file.
    // If no environment file is found or specified, the collection runs without an environment.
    // The default environment ('$shared') is used if no specific environment is set.
    // Environments can be switched dynamically at runtime.
    tp.SetEnvironment("local");

    // At the end of the collection run, a test results report is generated.
    // You can add a custom reporting method that will be triggered automatically.
    tp.RegisterReporter((summary) =>
    {
        Console.Write("Custom reporter report: ");

        // Tests results summary object has handy properties, which help with reporting.
        if (summary.AllTestsPassed)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Success! All ({summary.NumberOfExecutedTests}) tests passed.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Fail :( {summary.PercentageOfFailedTests}% tests failed.");
        }

        Console.ResetColor();
    });

    // For more advanced and customized reporting, use the following approach:
    // tp.RegisterReporter(IReporter<TestsResultsSummary> reporter);
    // The reporter must implement the public interface IReporter<TestsResultsSummary>.
}
