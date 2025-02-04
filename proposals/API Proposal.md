# API Proposal

## General introduction

In order to make this framework easy to use, there will be only one instance of public class `TeaPie`, which will hold all necessary functionality and information for user. `TeaPie` class will be designed as `Proxy` for end-user. Application will create instance in initialisation phase. Simplified example:

```csharp
TeaPie tp = new();
```

Then, end-user can approach all necessary attributes in following way:

```csharp
var body = tp.Request.Body;
```

## Testing API

Tests within this framework can be divided into two categories:

- **Determined tests** - without possibility to input parameters
- **Theoretical tests** - these tests are represented by testing scenario(s) with custom parameters

### Determined tests

#### API (Determined tests)

```csharp
public void Test(string testName, Action testFunction);
public async Task Test(string testName, Func<Task> testFunction); // Asynchronous test
```

#### Examples of usage (Determined tests)

```csharp
tp.Test("Status Code Should Be 200.", () => {
    tp.Response.StatusCode.Should().Be().Equal(200);
});
```

### Theoretical tests (Scenarios)

#### API (Theoretical tests)

```csharp
public void AddTestTheory(Action<Theory> testFunction);
public void AddTestTheory(string testName, Action<Theory> testFunction);
```

#### Examples of usage (Theoretical tests)

```csharp
tp.AddTestTheory(theory => {
    theory.Input<int>("m", 100); // Input parameter 'm' is set to 100
    theory.Input<int>("c", 5); // Input parameter 'c' is set to 5

    theory.Expected<int>("E", 2500); // Parameter 'E' is expected to be 2 500, after request execution
});

// Another theory for the same test case
tp.AddTestTheory(theory => {
    theory.Input<int>("m", 1);
    theory.Input<int>("c", 5);

    theory.Expected<int>("E", 25);
});
```

Method `tp.Test()` will be called in the **post-response** scripts in the most cases. Althought, there is no problem to use this method within **pre-request** script - e.g. when you want to check the state before request execution.

On the other hand method `tp.AddTestTheory()` must be located in **pre-request** script, other-wise it will be ignored.

## Variables API

User will be able to work with multi-level variables. Each variable name can contain alphabetical, numerical characters and dash (`"-"`).

End-user will be able to access variables either generally or on specific level.

1. `Global level`
2. `Environment level`
3. `Collection level` - this level exists only when running collection (not single test case only)
4. `Scope level`
5. `Test Case level`

The order of the levels determines in which order will be variables collected. The priority of variables rise with the level. So, for example if variable `foo` is defined on `global level` with the value `moo`, end-user can override its value on all levels from `environment level` to `test case level`.

Working with variables on specific level:

```csharp
// Approaching variables
var globalVar = tp.GlobalVariables.Get<int>("MyGlobalVariable");
var enviroVar = tp.EnvironmentVariables.Get<string>("MyEnvVariable");
var collVar = tp.CollectionVariables.Get<bool>("MyCollectionVariable");
var scopeVar = tp.ScopeVariables.Get<bool>("MyScopeVariable");
var testVar = tp.TestCaseVariables.Get<DateTime>("MyTestCaseVariable");

// Checking existence of variables
var globalVar = tp.GlobalVariables.Contains("MyGlobalVariable");
var enviroVar = tp.EnvironmentVariables.Contains("MyEnvVariable");
var collVar = tp.CollectionVariables.Contains("MyCollectionVariable");
var scopeVar = tp.ScopeVariables.Contains("MyScopeVariable");
var testVar = tp.TestCaseVariables.Contains("MyTestCaseVariable");

// Altering variables
tp.GlobalVariables.Set("MyGlobalVariable", 123);
tp.EnvironmentVariables.Set("MyEnvVariable", "Testlab");
tp.CollectionVariables.Set("MyCollectionVariable", true);
tp.TestCaseVariables.Set("MyTestCaseVariable", '2024-08-01');

// Removing variables
tp.GlobalVariables.Remove("MyGlobalVariable");
tp.EnvironmentVariables.Remove("MyEnvVariable");
tp.CollectionVariables.Remove("MyCollectionVariable");
tp.TestCaseVariables.Remove("MyTestCaseVariable");
```

Approaching variables generally:

```csharp
if (!tp.ContainsVariable(IsDevlabEnvironment)){
    tp.SetVariable("IsDevlabEnvironment", false);
}

var myVar = tp.GetVariable("IsDevlabEnvironment");
tp.RemoveVariable("IsDevlabEnvironment");
tp.RemoveVariables("Temp-Collection"); // Removes all variables with such a prefix
```

By approaching variables generally, method `GetVariable<T>()` will search for variable from highest level to lowest level (from `test case level` to `global level`) and will return first found. Method `SetVariable<T>()` will automatically store the variable on `scope level`. Methods `RemoveVariable()` and `RemoveVariables()` remove variables on all levels on which occur.

## Test Runner API

By default, runner takes all test cases and run them in alphabetical order by their names. Althought, you can manipulate with the order. You can do it  even during run-time. For these purposes, you can use these methods:

```csharp
public void NextTestCase(string pathToTestCase); // Test case that will run next
public void NextFolder(string pathToFolder); // Folder that will run next
public void SkipTestCase(string pathToTestCase); // Test case to be ignored/skipped
public void SkipTestCases(params string[] pathsToTestCases); // Test cases to be ignored/skipped
public void SkipFolder(string pathToFolder); // Folder to be skipped at all
```

These methods can be especially useful, when `Response` contains values that alter next behavior, or when tests failed and next request(s) don't make sense anymore.

**Be careful with these methods, since you can cause endless loop!**

If you want to specify order of tests at once, you can do it by using this method:

```csharp
tp.SetRunner(runner => {
    runner.SetOrder(
        "Test 1",
        "Test 25",
        "Auto Test");

    runner.Skip(
        "Ignor Me Test",
        "Test 0"
    );
})
```

## Execution Context API

For end-user it is necessary to work with the attributes of current `Request` and its `Response`. Currently executed request and its response is stored in these attributes:

```csharp
var req = tp.Request;
req.Headers.Add("Application-Type", "25");

...

var res = tp.Response;
var body = res.Body.ToJson();
tp.SetVariable("EntityId", body.Id);
```

Since, usually tests are run within collection, user should be able to access to older request/responses by its name. Naturally, this has one condition - the **request/response has to be named** in order to find it. Retrieval of these information wil be pretty straightforward:

```csharp
var oldResponse = tp.Responses["My First Request"];
var oldBody = oldResponse.Body.ToJson();
if (oldBody.TotalSum > 100){
    tp.Request.Body.ToJson().Add("DoNotRecalculate", true);
}else{
    tp.Request.Body.ToJson().Add("DoNotRecalculate", false);
}

var oldRequest = tp.Requests["My First Request"];
foreach(var header in oldRequest.Headers){
    tp.Request.Headers.Add(header.Key, header.Value);
}
```

## Logging API

Framework counts with detailed logging system. For end-use rit can be really helpful, when debugging and resolving what went wrong during test(s) execution. Therefore, user will be able to access to `ILogger` instance (from `Microsoft.Logging.Abstraction`) - by default it will be instance of `Serilog Logger`. This way, end-user will be able to write logs into all levels within scripts. Examples:

```csharp
tp.Logger.LogCritical("This is also Critical!");
tp.Logger.LogError("Error occured!");
tp.Logger.LogWarning("This is warning!");
tp.Logger.LogInformation("Today is nice weather :)");
tp.Logger.LogDebug("This will help me during debugging!");
tp.Logger.LogTrace("This is the most detailed message.");
tp.Logger.Log(LogLevel.None, new Exception("No level provided"), "This is {1}!", "ignored");
```

Full API of `ILogger` can be found [here](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.ilogger?view=net-8.0).

**Idea for future:** Let end-user specify his own intance of logger.

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var loggerFactory = LoggerFactory.Create(builder => {
    builder.AddSerilog();
});

tp.SetLogger(loggerFactory.CreateLogger<TeaPie>();)
```

## Reporting API

By default, reporting will be displayed on **console**. If user want to save report to file, there will be two options how to do so. Either he will put parameter `--report-output <path/to/destination>`, when calling CLI command or he will set it programmatically:

```csharp
tp.ReportingService.SaveOutputTo("./path/to/your/file.xml");
```

By default, the output file will have `JUnit XML` format.

**Idea for future:** Let end-user specify his own formatter of output file.
