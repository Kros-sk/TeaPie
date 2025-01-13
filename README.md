
# Installation

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

The tool should now be ready to use via the `teapie` command.

---

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

---

# Usage

To get started, **create your first test case** using the command:

```sh
teapie generate <test-case name> [path] [-i|--init|--pre-request] [-t|--test|--post-response]
```

This command generates three files in the specified path (or the current directory if no path is provided):
- **Pre-request script**: `<test-case-name>-init.csx`
- **Request file**: `<test-case-name>-req.http`
- **Post-response script**: `<test-case-name>-test.csx`

You can set the `-i` and `-t` options to `false` in order to **disable** the generation of the pre-request or post-response scripts.

---

## Pre-request Script

The **pre-request script** is used to set variables and initialize any required data before sending requests.

- Use the `#nuget` directive to install **NuGet packages**:
  ```csharp
  #nuget Package.Id, Version
  ```

  Even though NuGet packages are installed globally across all scripts, you must use the `using` directive to access them in your scripts.

- Access the **test runner context** using the globally available `tp` identifier:
  ```csharp
  tp.SetVariable("CurrentTime", DateTime.UtcNow);
  ```

- Reference other scripts using the `#load` directive. You can provide either an absolute or a relative path:
  ```csharp
  #load "path\to\your\script.csx"
  ```
  When using relative paths, the parent folder of the current script serves as the starting point. Note that the `#load` directive does not automatically execute the loaded script; it simply allows access to its functions.

---

## Request File

A **request file** can contain one or more HTTP requests. To separate requests, use the `###` comment line between two requests. You can also name your requests for easier management by adding a metadata line just before request definition:
```http
# @name RequestName
```

All variables can be used in the request file with the `{{variableName}}` notation. Ensure the `ToString()` method is overridden for reference types to ensure correct variable substitution.

For **named requests**, you can access request and response data using the following syntax:
```http
{{requestName.(request|response).(body|headers).(*|JPath|XPath)}}
```

This gives you comprehensive access to headers and body content of named requests.

---

## Post-response Script

The **post-response script** is used to define tests. A test is considered **failed** if an exception is thrown within the test body, which aligns with common practices in testing frameworks. This approach allows you to **use any assertion library** referenced via NuGet.

Example of a simple test:
```csharp
tp.Test("Status code should be 201.", () =>
{
    var statusCode = tp.Responses["RequestName"].StatusCode();
    statusCode.Should().Be(201);
});
```

- Use `tp.Requests` and `tp.Responses` to access requests and responses objects for named requests.
- For a single request in the file or the most recently executed request, you can directly use `tp.Request` and `tp.Response`.

Both `HttpRequestMessage` and `HttpResponseMessage` objects are enriched with the `GetBody` and `GetBodyAsync` methods to retrieve the body content as a string. Moreover, response object is extended by `StatusCode()` method, which easifies work with status codes by returning its **integer** value.

---

### JSON Handling

For requests that handle `application/json` payloads, a `ToJson()` extension method is available to simplify access to JSON properties:
```csharp
tp.Test("Identifier should be a positive integer.", () =>
{
    var responseBody = tp.Response.GetBody().ToJson();
    responseBody["id"].As<int>().Should().BeGreaterThan(0);
});
```

This makes working with JSON responses straightforward and efficient.
