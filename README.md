
# TeaPie

**TeaPie** is flexible CLI tool for API testing. The name comes from **TE**sting **API** **E**xtension. Since, you will write tests faster, you can enjoy some **tea** :tea: with **pie** :cake: in mean-time. :wink:

- [TeaPie](#teapie)
  - [Getting started](#getting-started)
  - [Usage](#usage)
    - [Test case](#test-case)
    - [Running Tests](#running-tests)
    - [Exploring Collection Structure](#exploring-collection-structure)
    - [Logging options](#logging-options)
    - [Pre-request Script](#pre-request-script)
    - [Request File](#request-file)
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

1. Run the application:

   ```sh
   dotnet run test "../../demo" # this will run 'teapie test' command on demo collection
   ```

You can **learn more** about how to use this tool either in [Usage section](#usage) or by checking attached [demo](./demo/).

## Usage

### Test case

To get started, **create your first test case** using the command:

```sh
teapie generate <test-case-name> [path] [-i|--init|--pre-request] [-t|--test|--post-response]
```

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

This command **runs all test cases** found in the current folder and its subfolders. For more advanced usage, the full command specification is:

```sh
teapie test [path-to-collection] [--temp-path <path-to-temporary-folder>] [-d|--debug] [-v|--verbose] [-q|--quiet] [--log-level <minimal-log-level>] [--log-file <path-to-log-file>] [--log-file-log-level <minimal-log-level-for-log-file>]
```

To view detailed information about each argument and option, use:

```sh
teapie --help
```

The **collection run** consists of two main steps:

1. **Structure Exploration** - The tool examines the folder structure to identify all test cases.

2. **Testing** - Each found test case is executed one by one.

### Exploring Collection Structure

If you only want to **inspect the collection structure** without running the tests, you can do so with the following command:

```sh
teapie explore [path-to-collection] [-d|--debug] [-v|--verbose] [-q|--quiet] [--log-level <minimal-log-level>] [--log-file <path-to-log-file>] [--log-file-log-level <minimal-log-level-for-log-file>]
```

### Logging options

- **Debug Output (`-d | --debug`)**: Displays more detailed logging.
- **Verbose Output (`-v | --verbose`)**: Displays the most detailed logging.
- **Quiet Mode (`-q | --quiet`)**: Suppresses any output.
- **Logging Options**:
  - **`--log-level`** - Sets the minimal log level for console output.
  - **`--log-file`** - Specifies a path to save logs.
  - **`--log-file-log-level`** - Sets the minimal log level for the log file.

### Pre-request Script

The **pre-request script** is used to set variables and initialize any required data before sending requests.

- Use the `#nuget` directive to install **NuGet packages**:

  ```csharp
  #nuget "AutoBogus, 2.13.1"
  ```

  >💁‍♂️ Even though NuGet packages are installed globally across all scripts, you must use the `using` directive to access them in your scripts.

- Access the **test runner context** using the globally available `tp` identifier:

  ```csharp
  tp.SetVariable("CurrentTime", DateTime.UtcNow);
  ...
  var time = tp.GetVariable("CurrentTime");
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

### Post-Response Script

The **post-response script** is used to **define tests**. A test is considered **failed** if an exception is thrown within the test body, following standard testing framework practices. This approach allows you to **use any assertion library** referenced via NuGet.

<!-- omit from toc -->
#### Example Test

```csharp
tp.Test("Status code should be 201.", () =>
{
    var statusCode = tp.Response.StatusCode();
    Equal(statusCode, 201);
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
    var statusCode = tp.Response.StatusCode();
    Equal(statusCode, 201);
}, true); // Skip this test
```

<!-- omit from toc -->
#### Asynchronous Tests

Asynchronous tests are fully supported:

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

- **`GetBody()` / `GetBodyAsync()`** - Retrieves the body as a `string`.
- **`GetBody<TResult>()` / `GetBodyAsync<TResult>()`** - Deserializes the JSON body into an object of type `TResult`.
- **`GetBodyAsExpando()` / `GetBodyAsExpandoAsync()`** - Retrieves the body as a **case-insensitive `dynamic` expando object**, making property access easier.
  - *Note*: To use an expando object correctly, explicitly declare it as `dynamic`.

<!-- omit from toc -->
#### Status Code Handling

The response object includes a `StatusCode()` method that simplifies status code handling by returning its **integer** value.

<!-- omit from toc -->
#### JSON Handling

For requests that handle `application/json` payloads, a **extension methods** `ToExpando()` and `ToJson()` can simplify access to JSON properties:

```csharp
// Using case-insensitive expando object
tp.Test("Identifier should be a positive integer.", () =>
{
    // Expando object has to be marked epxlicitly as 'dynamic'
    dynamic responseBody = tp.Response.GetBody().ToExpando();
    True(responseBody.id > 0);
});

// Using JObject
tp.Test("Identifier should be a positive integer.", () =>
{
    var responseBody = tp.Response.GetBody().ToJson();
    True(responseBody["id"].As<int>() > 0);
});
```

This makes working with JSON responses straightforward and efficient.

### Environments

Environments are a crucial part of automating tests, allowing you to define variables for different scenarios. **TeaPie** supports environments to enhance flexibility and efficiency.

<!-- omit from toc -->
#### Environment File

To use environments, you need to define them in **environment file** - a JSON file located within the **collection folder structure**. By default, the tool uses the **first found file** (depth-first algorithm) with name `<collection-name>-env.json`. However, you can specify a custom environment file by using the following option:

```sh
--env-file <path-to-environment-file>
```

> You can use alias `--environment-file`, too.

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

There are some scenarios where you want to **switch environment in the code** (`.csx` scripts). There you can use:

```csharp
tp.SetEnvironment("local");
```

### Reporting

At the end of a collection testing run, a summary report of tests' results is generated. Here is an example:

![Report example](./assets/images/report-example.png)

<!-- omit from toc -->
#### Custom Reporters

The default console reporter, powered by `AnsiConsole` from `Spectre.Console`, provides all essential tests' results details. However, users can **add custom reporters**, either by defining them **inline**:

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

> All necessary information about results of tests can be found within [TestsResultsSummary](./src/TeaPie/Reporting/TestsResultsSummary.cs) object. The summary contains properties with access to commonly evaluated statistics as `AllTestsPassed`, `NumberOfFailedTests`, `PercentageOfSkippedTests`, `FailedTests`, ...

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
