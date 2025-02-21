// INITIALIZATION SCRIPT

// Initialization script is executed before the first test case.
// To specify a custom initialization script, use the '--init-script <path-to-script>' option.
// If no script is explicitly set, the first detected 'init.csx' script is used by default.
// When an explicit script is provided, the default initialization script is ignored.

// ENVIRONMENTS

// By default, environments are defined in a '<collection-name>-env.json' file.
// To specify a custom environment file, use '--env-file <path-to-environment-file>'.
// If no environment file is found or specified, the collection runs without an environment.
// The default environment ('$shared') is used if no specific environment is set.
// Environments can be switched dynamically at runtime.
tp.SetEnvironment("local");

// LOGGER

// The logger, implementing Microsoft's ILogger, is available in all scripts.
tp.Logger.LogInformation("Starting demo collection testing...");

// AUTHENTICATION

// OAuth2 authentication is natively supported. To use this provider, configure it first.
// Body parameters defined by RFC 6749 are supported.
var authUrl = tp.GetVariable<string>("AuthServerUrl");
tp.ConfigureOAuth2Provider(new OAuth2OptionsBuilder()
    .WithAuthUrl(authUrl)
    .WithClientId("test-client")
    .WithGrantType("client_credentials")
    .WithClientSecret("test-secret")
    .AddParameter("custom_parameter", "true")
    .Build()
);

// Custom authentication providers can be also registered.
tp.RegisterAuthProvider(
    "MyAuth",
    new MyAuthProvider(tp.ApplicationContext)
        .ConfigureOptions(new MyAuthProviderOptions { AuthUrl = authUrl })
);

// Sets OAuth2 as the default authentication provider. This means that specified authentication provider will be
// applied on all requests, unless explictly set otherwise (by ## AUTH-PROVIDER: AuthProvider directive in .http file).
tp.SetDefaultAuthProvider("OAuth2");

// RETRY STRATEGIES

// Register custom retry strategies that can be referenced across multiple requests.
tp.RegisterRetryStrategy("Default retry", new RetryStrategyOptions<HttpResponseMessage>
{
    MaxRetryAttempts = 3,                         // Maximum retry attempts
    Delay = TimeSpan.FromMilliseconds(500),       // Initial delay before retrying
    MaxDelay = TimeSpan.FromSeconds(2),           // Maximum delay between retries
    BackoffType = DelayBackoffType.Exponential,   // Backoff strategy (e.g., Linear, Exponential)
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .HandleResult(response => true)          // Retries for all responses (adjust condition as needed)
});

// Another example with minimal configuration.
tp.RegisterRetryStrategy("Custom retry", new() { MaxRetryAttempts = 2 });

// REPORTING

// At the end of the collection run, a test results report is generated.
// Users can add a custom reporting method that will be triggered automatically.
tp.RegisterReporter(summary =>
{
    Console.Write("Custom reporter report: ");

    // The summary object provides useful properties for reporting.
    if (summary.AllTestsPassed)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Success! All {summary.NumberOfExecutedTests} tests passed.");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Failure: {summary.PercentageOfFailedTests:F2}% of tests failed.");
    }

    Console.ResetColor();
});

// For more advanced and customized reporting, use:
// tp.RegisterReporter(IReporter<TestsResultsSummary> reporter);
// The reporter must implement the IReporter<TestsResultsSummary> interface.

// CUSTOM CLASS DEFINITIONS

// Custom authentication provider definition
public class MyAuthProvider(IApplicationContext context) : IAuthProvider<MyAuthProviderOptions>
{
    private readonly IApplicationContext _context = context;
    private MyAuthProviderOptions _options = new();

    public async Task Authenticate(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _context.Logger.LogInformation("{Provider}: Authentication completed via '{AuthUrl}'", nameof(MyAuthProvider), _options.AuthUrl);
        await Task.CompletedTask;
    }

    public IAuthProvider<MyAuthProviderOptions> ConfigureOptions(MyAuthProviderOptions options)
    {
        _options = options;
        return this;
    }
}

// Custom authentication provider options definition
public class MyAuthProviderOptions : IAuthOptions
{
    public string AuthUrl { get; set; } = string.Empty;
}
