// Any script named 'init.csx' within the collection is treated as an initialization script
// and is automatically executed before the first test case.
//
// Users can specify a custom initialization script using the --init-script option.
// If no script is explicitly set, the first detected 'init.csx' script is used by default.
// When an explicit script is provided, the default initialization script is ignored.

// INITIALIZATON:

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
        Console.WriteLine($"Fail :( {summary.PercentageOfFailedTests.ToString("f2")}% tests failed.");
    }

    Console.ResetColor();
});

// For more advanced and customized reporting, use the following approach:
// tp.RegisterReporter(IReporter<TestsResultsSummary> reporter);
// The reporter must implement the public interface IReporter<TestsResultsSummary>.

// Logger implementing Microsoft's ILogger is accessible everywhere in the scripts.
tp.Logger.LogInformation("Start of demo collection testing.");

// Users can register custom retry strategies that can be reused across multiple requests by referencing them by name.
tp.RegisterRetryStrategy("Default retry", new RetryStrategyOptions<HttpResponseMessage>
{
    MaxRetryAttempts = 3,
    Delay = TimeSpan.FromMilliseconds(500),
    MaxDelay = TimeSpan.FromSeconds(2),
    BackoffType = DelayBackoffType.Exponential,
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .HandleResult(response => true)    // Condition determining whether a request should be retried. This one retries always.
});
