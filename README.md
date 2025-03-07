
# TeaPie

**TeaPie** is flexible CLI tool for API testing. The name comes from **TE**sting **API** **E**xtension. Since, you will write tests faster, you can enjoy some **tea** :tea: with **pie** :cake: in mean-time. :wink:

- [TeaPie](#teapie)
  - [Getting started](#getting-started)
  - [Usage](#usage)
    - [Test case](#test-case)
    - [Running Tests](#running-tests)
    - [Exploring Collection Structure](#exploring-collection-structure)
    - [Logging](#logging)
    - [Initialization script](#initialization-script)
    - [Pre-request Script](#pre-request-script)
    - [Request File](#request-file)
    - [Authentication](#authentication)
    - [Retrying](#retrying)
    - [Post-Response Script](#post-response-script)
    - [Environments](#environments)
    - [Reporting](#reporting)
  - [How to install locally](#how-to-install-locally)
    - [Setting up a Local NuGet Feed](#setting-up-a-local-nuget-feed)

## Getting started

Since `TeaPie.Tool` is not on NuGet store yet, the easiest way to launch this tool is:

1. Move to the **project folder**:

   ```sh
   cd ./src/TeaPie.DotnetTool
   ```

2. Run test of the `demo` **collection**:

   ```sh
   dotnet run test "../../demo" # this will run 'teapie test' command on demo collection
   ```

   Alternatively, you can run just **single test case**:

   ```sh
    dotnet run test "../../demo/Tests/2. Cars/EditCar-req.http" -i "../../demo/init.csx" --env-file "../../demo-env.json"
   ```

You can **learn more** about how to use this tool either in [Usage section](#usage) or by checking attached [demo](./demo/).

## Usage

### Test case

To get started, **create your first test case** using the command:

```sh
teapie generate <test-case-name> [path] [-i|--init|--pre-request] [-t|--test|--post-response]
```

> 💁‍♂️ You can use aliases `gen` or `g` to run the same command.

This command generates three files in the specified path (or the current directory if no path is provided):

- [**Pre-request script**](#pre-request-script): `<test-case-name>-init.csx`
- [**Request file**](#request-file): `<test-case-name>-req.http`
- [**Post-response script**](#post-response-script): `<test-case-name>-test.csx`

You can set the `-i` and `-t` options to `false` in order to **disable the generation of the pre-request or post-response scripts.**

### Running Tests

After generating test cases and writing your tests, you can execute the **main command for testing**:

```sh
teapie
```
TeaPie supports two execution modes:

- **Collection Run** - If a **directory path** is provided, tool runs all **test-cases** found in the specified folder and its subfolders.
- **Single Test-Case Run** - If a `.http` **file path** is provided, then tool executes **only that specific test-case**.

For more advanced usage, here’s the full command specification:

```sh
teapie test [path-to-collection] [--temp-path <path-to-temporary-folder>] [-d|--debug] [-v|--verbose] [-q|--quiet] [--log-level <minimal-log-level>] [--log-file <path-to-log-file>] [--log-file-log-level <minimal-log-level-for-log-file>] [-e|--env|--environment <environment-name>] [--env-file|--environment-file <path-to-environment-file>] [-r|--report-file <path-to-report-file>] [-i|--init-script|--initialization-script <path-to-initialization-script>]
```

> 💁‍♂️ You can use alias `t` or **completely omit command name**, since `test` command is considered as **default command** when launching `teapie`.

To view detailed information about each argument and option, run:

```sh
teapie --help
```

Both single test-case and collection runs follow these two main steps:

1. **Structure Exploration** – TeaPie scans the directory or test-case structure to identify all test cases and related files.
2. **Test Execution** – Each detected test is executed based on the provided configuration.

### Exploring Collection Structure

If you only want to **inspect the collection or test-case structure** without running the tests, you can do so with the following command:

```sh
teapie explore [path-to-collection-or-test-case] [-d|--debug] [-v|--verbose] [-q|--quiet] [--log-level <minimal-log-level>] [--log-file <path-to-log-file>] [--log-file-log-level <minimal-log-level-for-log-file>] [--env-file|--environment-file <path-to-environment-file>] [-i|--init-script|--initialization-script <path-to-initialization-script>]
```

> 💁‍♂️ You can use aliases `exp` or `e` to run the same command.

### Logging

Logging is essential part of any application. The main logger is exposed as `ILogger` from `Microsoft.Extensions.Logging` and user can use it easily by accessing it via `tp` instance:

```csharp
tp.Logger.LogInformation("I understand logging in TeaPie! Yee!");
```

By default, `TeaPie` uses `Serilog` as the logging provider.

Users can adjust logging levels during application run by using these options:

- **Debug Output (`-d | --debug`)**: Displays more detailed logging.
- **Verbose Output (`-v | --verbose`)**: Displays the most detailed logging.
- **Quiet Mode (`-q | --quiet`)**: Suppresses any output.
- **Logging Options**:
  - **`--log-level`** - Sets the minimal log level for console output.
  - **`--log-file`** - Specifies a path to save logs.
  - **`--log-file-log-level`** - Sets the minimal log level for the log file.

### Initialization script

Before the (first) test case is executed, users have the opportunity to run an **initialization script**. This script is intended for **pre-test setup** tasks such as *setting environment variables, defining reporters, configuring logging,* and more.

By default, **first found** script **within collection/parent folder of the test-case** with name `init.csx` is used. However, users can **explicitly specify** a different script by using the following option:

```sh
-i|--init-script|--initialization-script <path-to-script>
```

### Pre-request Script

The **pre-request script** is used to set variables and initialize any required data before sending request(s).

- Use the `#nuget` directive to install **NuGet packages**:

  ```csharp
  #nuget "AutoBogus, 2.13.1"
  ```

  >💁‍♂️ Even though NuGet packages are installed globally across all scripts, you must use the `using` directive to access them in your scripts.

- Access the **test runner context** using the globally available `tp` identifier:

  ```csharp
  tp.SetVariable("TimeOfExecution", DateTime.UtcNow);
  ...
  var time = tp.GetVariable("TimeOfExecution");
  ```

- Reference other scripts using the `#load` directive. You can provide either an absolute or a relative path.

  **IMPORTANT:** Referenced script is **automatically executed**. For this reason, rather encapsulate logic in methods, to prevent unwanted execution.

  ```csharp
  #load "path\to\your\script.csx"
  ```

  >💁‍♂️ When using relative paths, the parent folder of the current script serves as the starting point.

### Request File

A [**request file**](https://learn.microsoft.com/en-us/aspnet/core/test/http-files?view=aspnetcore-9.0) can contain one or more HTTP requests. To separate requests, use the `###` comment line between two requests. You can also name your requests for easier management by adding a metadata line just before request definition:

```http
# @name RequestName
GET https:/localhost:3001/customers
Content-Type: application/json

{
    "Id": 3,
    "FirstName": "Alice",
    "LastName": "Johnson",
    "Email": "alice.johnson@example.com"
}
```

All variables can be used in the request file with the `{{variableName}}` notation.

>💁‍♂️ When you want to use **reference types for variables**, make sure that they override `ToString()` method. During variable resolution, `ToString()` will be called on them.

For **named requests**, you can access request and response data using the following syntax:

```http
{{requestName.(request|response).(body|headers).(*|JPath|XPath)}}

# Example that will fetch 'Id' property from 'AddNewCarRequest' request's JSON body
{{AddNewCarRequest.request.body.$.Id}}
```

This gives you comprehensive access to headers and body content of named requests.

<!-- omit from toc -->
### Test Directives

TeaPie provides **pre-defined test directives** that can be applied within `.http` files to automate response validation.
Additionally, users can **register custom test directives**, enabling more flexible and reusable test configurations.

<!-- omit from toc -->
#### Predefined Test Directives

TeaPie supports the following built-in test directives:

- `## TEST-EXPECT-STATUS: [200, 201]` – Ensures the response status code matches any value in the array.
- `## TEST-HAS-BODY` (Equivalent to `## TEST-HAS-BODY: True`) – Checks if the response contains a body.
- `## TEST-HAS-HEADER: Content-Type` – Verifies that the specified header is present in the response.

Example usage in a `.http` file:

```http
## TEST-EXPECT-STATUS: [200, 201]
## TEST-HAS-BODY
## TEST-HAS-HEADER: Content-Type
PUT {{ApiBaseUrl}}{{ApiCarsSection}}/{{AddCarRequest.request.body.$.Id}}
Content-Type: {{AddCarRequest.request.headers.Content-Type}}

...
```

<!-- omit from toc -->
#### Registering Custom Test Directives

TeaPie allows users to **define and register custom test directives** dynamically.
To register a **custom test directive**, use the following method:

```csharp
tp.RegisterTestDirective(
    string directiveName, // Directive name (excluding 'TEST-' prefix)
    string directivePattern, // Regular expression pattern for parser to recognize the directive
    Func<IReadOnlyDictionary<string, string>, string> testNameGetter, // Function to generate the test name based on input parameters
    Func<HttpResponseMessage, IReadOnlyDictionary<string, string>, Task> testFunction // Function to execute when the directive is applied
);
```

The following example registers a custom directive, `## TEST-CUSTOM: <true|false>`:

```csharp
tp.RegisterTestDirective(
    "CUSTOM", // Name of the directive (excluding 'TEST-' prefix)
    TestDirectivePatternBuilder.Create("CUSTOM") // For Regex pattern generation it is recommended to use 'TestDirectivePatternBuilder'
        .AddBooleanParameter("MyBool") // Users can add multiple parameters, all with different data types. Default deparator between parameters is ';'.
        .Build(),
    (parameters) => {
        var negation = bool.Parse(parameters["MyBool"]) ? string.Empty : "NOT "
        $"Response status code should {negation}be successful."
    },
    async (response, parameters) =>
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
```

Now, the directive is ready to be used in a `.http` file:

```http
## TEST-CUSTOM: True
GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{RentCarRequest.request.body.$.CarId}}
```

### Authentication

To provide maximum flexibility, an **authentication interceptor** is applied to all outgoing HTTP requests. The **authentication provider** to be used within interceptor **can be specified in scripts** or directly within **request files**.

<!-- omit from toc -->
#### Available Authentication Providers

Currently, **TeaPie** supports two authentication providers:

- `None` - **No authentication** is performed. This is the **default behavior** for all requests.
- `OAuth2` - A commonly used authentication method, **natively supported** by the tool.

To use **OAuth2**, it must be configured before executing requests:

```csharp
tp.ConfigureOAuth2Provider(OAuth2OptionsBuilder.Create()
    .WithAuthUrl(tp.GetVariable<string>("AuthServerUrl")) // Required parameter.
    .WithGrantType("client_credentials") // Required parameter.
    .WithClientId("test-client")
    .WithClientSecret("test-secret")
    .AddParameter("custom_parameter", "true") // Add custom parameters if needed.
    .Build()
);
```

<!-- omit from toc -->
#### Registering a Custom Authentication Provider

To use a custom authentication provider, **register it before usage**:

```csharp
tp.RegisterAuthProvider(
    "MyAuth",
    new MyAuthProvider(tp.ApplicationContext)
        .ConfigureOptions(new MyAuthProviderOptions { AuthUrl = authUrl })
);
```

If the **registered provider should also be the default**, use this method instead:

```csharp
tp.RegisterDefaultAuthProvider(
    "MyAuth",
    new MyAuthProvider(tp.ApplicationContext)
        .ConfigureOptions(new MyAuthProviderOptions { AuthUrl = authUrl })
);
```

<!-- omit from toc -->
#### Setting a Default Authentication Provider

To specify which **registered authentication provider** should be used for all requests, set it as the default:

```csharp
tp.SetDefaultAuthProvider("MyAuth"); // Sets 'MyAuth' as the default authentication provider.
```

For `OAuth2` there is built-in method:

```csharp
tp.SetOAuth2AsDefaultAuthProvider();
```

If no authentication provider is explicitly set as default, requests will **default to "None"**, meaning no authentication is applied.

<!-- omit from toc -->
#### Using a Specific Authentication Provider for a Request

Some requests may require a different authentication mechanism than the default.
To **assign a specific authentication provider** for a request, use this directive in a `.http` file:

```http
## AUTH-PROVIDER: MyAuth
POST {{ApiBaseUrl}}{{ApiCarsSection}}
Content-Type: application/json

...
```

<!-- omit from toc -->
#### Disabling Authentication

By default, no authentication is performed. However, if a default authentication provider is set, it applies to all requests.
To **disable authentication** for a specific request, use the pre-defined authentication provider 'None':

```http
## AUTH-PROVIDER: None
POST {{ApiBaseUrl}}{{ApiCarsSection}}
Content-Type: application/json

...
```

### Retrying

TeaPie allows users to **register named retry strategies**, which can be referenced later for request retrying. These strategies define **how and when** requests should be retried. The structure of the retry strategy is defined by the object `RetryStrategyOptions<HttpResponseMessage>` from [Polly.Core](https://github.com/App-vNext/Polly) library.

<!-- omit from toc -->
#### Registering a Retry Strategy

To create a reusable retry strategy, register it in a script:

```csharp
tp.RegisterRetryStrategy("Default retry", new RetryStrategyOptions<HttpResponseMessage>
{
    MaxRetryAttempts = 3, // Maximum number of retry attempts
    Delay = TimeSpan.FromMilliseconds(500),  // Default delay between retries.
    BackoffType = DelayBackoffType.Exponential, // Defines how delay increases (e.g., Linear, Exponential)
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError) // Condition determining whether a request should be retried (Retry, when status code is 500).

    // More properties can be configured (see 'Polly.Core.Retry.RetryStrategyOptions<TResult>' for more information)
});
```

<!-- omit from toc -->
#### Using Retry Directives in `.http` Files

Within `.http` files, you can apply a retry strategy using **retry directives**. These directives allow fine-tuning of request retrying behavior.

```http
# @name GetCarRequest
# Applying a named retry strategy and overriding specific properties.
## RETRY-STRATEGY: Default retry   # Uses already registered "Default retry" strategy
## RETRY-MAX-ATTEMPTS: 5           # Overrides max retry attempts
## RETRY-BACKOFF-TYPE: Linear      # Changes backoff strategy for this request
## RETRY-MAX-DELAY: 00:00:03       # Limits the maximum delay between retries
## RETRY-UNTIL-STATUS: [200, 201]  # Adds condition - retry until one of given status codes is received
GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{RentCarRequest.request.body.$.CarId}}
```

>💁‍♂️ These **modifications apply only to the current request** and do not alter the registered retry strategy. If **no retry strategy is selected, default one** (from `Polly.Core`) is taken and modified accordingly.

Using retry strategies ensures **more resilient test execution**, handling **temporary failures** gracefully while preventing unnecessary retries.

### Post-Response Script

The **post-response script** is used to **define tests**. A test is considered **failed** if an exception is thrown within the test body, following standard testing framework practices. This approach allows you to **use any assertion library** referenced via NuGet.

> 💁‍♂️ However, the **natively supported assertion library** is `Xunit.Assert`, which is statically imported in all script files. This means you don't need the `Assert.` prefix to access its methods.

<!-- omit from toc -->
#### Example Test

```csharp
tp.Test("Status code should be 201.", () =>
{
    var statusCode = tp.Response.StatusCode();
    Equal(201, statusCode);
});
```

<!-- omit from toc -->
#### Accessing Requests and Responses

- For **single requests** or the **most recently executed request**, use `tp.Request` and `tp.Response`.
- For **multiple requests** in a `.http` file, use `tp.Requests` and `tp.Responses` to access named requests and responses.

<!-- omit from toc -->
#### Skipping Tests

During development or debugging, you may need to skip certain tests. To do this, set the optional `skipTest` parameter to `true`:

```csharp
tp.Test("Status code should be 201.", () =>
{
    var statusCode = tp.Responses["CreateCarRequest"].StatusCode();
    Equal(201, statusCode);
}, true); // Skip this test
```

<!-- omit from toc -->
#### Asynchronous Tests

Asynchronous tests are also fully supported:

```csharp
await tp.Test($"Newly added car should have '{brand}' brand.", async () =>
{
    var body = tp.GetVariable<string>("NewCar");
    dynamic obj = body.ToExpando();

    dynamic responseJson = await tp.Responses["GetNewCarRequest"].GetBodyAsExpandoAsync();
    Equal(obj.Brand, responseJson.brand);
});
```

<!-- omit from toc -->
#### Working with Body Content

Both `HttpRequestMessage` and `HttpResponseMessage` objects include convenient methods for handling body content:

- `GetBody()` / `GetBodyAsync()` - Retrieves the body as a `string`.
- `GetBody<TResult>()` / `GetBodyAsync<TResult>()` - Deserializes the JSON body into an object of type `TResult`.
- `GetBodyAsExpando()` / `GetBodyAsExpandoAsync()` - Retrieves the body as a **case-insensitive `dynamic` expando object**, making property access easier.
  - **IMPORTANT**: To use an expando object correctly, **explicitly declare containing variable** as `dynamic`.

<!-- omit from toc -->
#### Status Code Handling

The response object includes a `StatusCode()` method that simplifies status code handling by returning its **integer** value.

<!-- omit from toc -->
#### JSON Handling

For requests that handle `application/json` payloads, a **extension method** `ToExpando()` can simplify access to JSON properties:

```csharp
// Using case-insensitive expando object
tp.Test("Identifier should be a positive integer.", () =>
{
    // Expando object has to be marked epxlicitly as 'dynamic'
    dynamic responseBody = tp.Response.GetBody().ToExpando();
    True(responseBody.id > 0);
});
```

This makes working with JSON responses straightforward and efficient.

### Environments

Environments are a crucial part of automating tests, allowing you to define variables for different scenarios. **TeaPie** supports environments to enhance flexibility and efficiency.

<!-- omit from toc -->
#### Environment File

To use environments, firstly you must define them in a JSON **environment file**. By default, the tool uses the **first found file** within collection (depth-first algorithm) with name `<collection-name>-env.json`, respectively first found file in the parent directory of provided test-case which follows `<test-case-name>-env.json` naming convention, when running single test-case. However, you can specify a custom environment file by using the following option:

```sh
--env-file|--environment-file <path-to-environment-file>
```

This is example, of how environment file can look like:

```json
{
    "$shared": {
        "ApiBaseUrl": "http://my-car-rental-company.com",
        "ApiCustomersSection": "/customers",
        "ApiCarsSection": "/cars",
        "ApiCarRentalSection": "/rental"
    },
    "local": {
        "ApiBaseUrl": "http://localhost:3001", // Override $shared's variable
        "DebugMode": true // Environment-specific variable
    }
}
```

Each environment is defined by its **name** and **set of variables**.

<!-- omit from toc -->
#### Default Environment (`$shared`)

Each environment file **should include** a `$shared` environment, which serves as the **default environment**. Key points about `$shared`:

- **Global Variables**: Variables from `$shared` are always stored in `tp.GlobalVariables`.
- **Environment Variables**: Variables from `$shared` are added to `tp.EnvironmentVariables` only if `$shared` is **selected as the active environment**.
- **Overwriting**: Other environments can override variables defined in `$shared`.

This approach was inspired by [Rest Client for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=humao.rest-client#environments).

<!-- omit from toc -->
#### Active Environment

To specify the environment for running tests, use the `-e` option followed by the environment name:

```sh
-e local
```

> You can also use aliases `--env` and `--environment` for the same purpose.

There are some scenarios where you want to **switch environments in the code** (`.csx` scripts). There you can use:

```csharp
tp.SetEnvironment("local");
```

### Reporting

<!-- omit from toc -->
#### Console report

At the end of a collection testing run, a summary report of test results is **automatically displayed to console** using `Spectre.Console` components. Here is an example:

![Report example](./assets/images/report-example.png)

<!-- omit from toc -->
#### Reporting to File

TeaPie includes a built-in **`JUnit XML` file reporter**, which can be enabled by adding the `-r|--report-file` **option** with a valid path to an `.xml` file where the report will be generated.

This **widely accepted format** is supported natively by **GitHub Actions** and **Microsoft Azure DevOps**. However, it is not fully standardized, and different CI tools may use modified versions of this format. For more details, refer to this [post](https://github.com/testmoapp/junitxml).

Since `JUnit XML` uses **different terminology** than `TeaPie`, here is the mapping:

- **`testsuites`** → **Collection**
- **`testsuite`** → **Test Case**
- **`testcase`** → **Test**

Time is automatically converted to **seconds** (a common practice) with **three decimal places**, using **dot notation**.

<!-- omit from toc -->
#### Custom Reporters

The **default console reporter**, powered by `AnsiConsole` from `Spectre.Console`, provides all essential test results details. However, users can **add custom reporters**, either by defining them **inline**:

```csharp
tp.RegisterReporter((summary) =>
{
    if (summary.AllTestsPassed)
    {
        Console.WriteLine($"Success! All {summary.NumberOfExecutedTests} tests passed.");
    }
    else
    {
        Console.WriteLine($"Failure: {summary.PercentageOfFailedTests}% of tests failed.");
    }
});
```

Or for more advanced and customizable reporting, by implementing a custom reporter class that implements the `IReporter<TestsResultsSummary>` interface:

```csharp
public class MyReporter : IReporter<TestsResultsSummary>
{
    public void Report(TestsResultsSummary summary)
    {
        // Custom reporting logic...
    }
}

tp.RegisterReporter(new MyReporter());
```

> All necessary information about results of tests can be found within [TestResultSummary](./src/TeaPie/Reporting/TestResultSummary.cs) object. The summary contains properties with access to commonly evaluated statistics as `AllTestsPassed`, `NumberOfFailedTests`, `PercentageOfSkippedTests`, `FailedTests`, ...

## How to install locally

To set up the application, execute the following commands:

1. Navigate to the project directory:

   ```sh
   cd "..\src\TeaPie.DotnetTool"
   ```

2. Pack the project in its `Release` version:

   ```sh
   dotnet pack -c Release
   ```

3. Copy the `.nupkg` file to your local NuGet feed (adjust the version number if needed):

   ```sh
   copy ".\bin\Release\TeaPie.Tool.1.0.0.nupkg" "path\to\your\local\nuget\feed"
   ```

4. Install the tool globally on your system:

   ```sh
   dotnet tool install -g TeaPie.Tool
   ```

The tool should be ready to use via the `teapie` command now.

### Setting up a Local NuGet Feed

If you don’t have a local NuGet feed already, you can set one up as follows:

1. Create a directory for the feed:

   ```sh
   mkdir "path/to/your/new/local/feed/directory"
   ```

2. Add the new directory as a NuGet source:

   ```sh
   dotnet nuget add source "path/to/your/local/feed" --name NameOfYourLocalFeed
   ```
