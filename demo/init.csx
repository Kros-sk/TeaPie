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
var authUrl = tp.GetVariable<string>("AuthServerUrl");
tp.ConfigureOAuth2Provider(OAuth2OptionsBuilder.Create()
    .WithAuthUrl(authUrl) // Required parameter.
    .WithGrantType("client_credentials") // Required parameter.
    .WithClientId("test-client")
    .WithClientSecret("test-secret")
    .AddParameter("custom_parameter", "true") // Add another/custom parameters.
    .Build()
);

// Custom authentication providers can be also registered.
tp.RegisterAuthProvider(
    "MyAuth",
    new MyAuthProvider(tp.ApplicationContext)
        .ConfigureOptions(new MyAuthProviderOptions { AuthUrl = authUrl })
);

// Sets OAuth2 as the default authentication provider. This means that OAuth2 authentication provider will be
// applied on all requests, unless explictly set otherwise (by ## AUTH-PROVIDER: AuthProvider directive in .http file).
tp.SetOAuth2AsDefaultAuthProvider(); // equals to: tp.SetDefaultAuthProvider("OAuth2");

// To set custom - already registered - authentication provider use method:
// tp.SetDefaultAuthProvider("NameOfCustomAuthProvider");

// There is also one useful method, which encapsulates registration process of authentication provider altogether
// with setting it as default provider:
// tp.RegisterDefaultAuthProvider("MyAuth2", new MyAuth2(tp.ApplicationContext));

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

// CUSTOM TEST DIRECTIVE

// TeaPie allows users to define custom test directives for '.http' files.
// Syntax of testing directives: ## TEST-CUSTOM-NAME: Parameter1; Parameter2; ...
// A custom directive can be registered as follows:
tp.RegisterTestDirective(
    "SUCCESSFUL-STATUS",
    TestDirectivePatternBuilder
        .Create("SUCCESSFUL-STATUS")  // Defines the directive name (excluding 'TEST-' prefix - left side)
        .AddBooleanParameter("MyBool")  // Adds a boolean parameter (multiple parameters can be added - right side)
        .Build(),  // Generates Regex pattern for this directive
    (parameters) =>  // Function that generates the test name using dictionary of available parameters
    {
        var negation = bool.Parse(parameters["MyBool"]) ? string.Empty : "NOT ";
        return $"Response status code should {negation}be successful.";
    },
    async (response, parameters) =>  // Asynchronous test function that validates the response
    {
        if (bool.Parse(parameters["MyBool"]))
        {
            True(response.IsSuccessStatusCode);
        }
        else
        {
            False(response.IsSuccessStatusCode);
        }

        await Task.CompletedTask;
    }
);

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
