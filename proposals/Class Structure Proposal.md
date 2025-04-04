# Class Structure

## Public class

------------------
For end-user there will be only one visible class - `TeaPie`

### `TeaPie`

This class will be proxy for user - it will hold whole API. End-user will control whole application by executing methods and accessing properties of this class. We can split this class according logical contexts to these segments:

- `Runner` - encapsulates all manipulation with the run of test cases. Such as order changing, skipping, retrying or adjusting delays and timeouts.
- `Testing section` - with these methods, end-user will be able to write and run own API tests.
- `Execution Context` - this will hold all information about current requests and responses. End-user will have possibility to read, as well as modify them.
- `Variables` - end-user will be able to get, set and remove variables on different levels (hierarchically organized).
- `Logger` - enables end-user to create logs during execution.
- `ReportingService` - for adjusting reporting settings.

This segmentation is similar to actual division of classes.

## Structure Explorer

### `StructureExplorer`

Is responsible for understanding folder and test-case structure. These two methods are crucial:

```csharp
public static TestCaseStructure ExploreTestCase(string testCaseName);
public static CollectionStructure ExploreCollection(string collectionName);
```

Method `ExploreTestCase(string)` will check for implicit pre-request/post-response scripts and configuration file.

Method `ExploreCollection(string)` will cexamine folder structure and if it finds test case, `ExploreTestCase(string)` will be called. Also, it will search for configuration files for collection. As a result, it will provide whole tree-like structure with information about single test cases.

## Runner

Since we want to support two types of run, there will be two classes:

### `TestCaseRunner`

Will have one main method:

```csharp
public TestCaseRunResults Run(TestCaseStructure structure)
```

Which will run one test case. As input it will take structure of the test case - this desrcibes if there are implicit pre-request script, post-response script and configuration file.

### `CollectionRunner`

Will also have main method for running:

```csharp
public CollectionRunResults Run(CollectionStructure structure)
```

Which will run whole collection. As input it will take structure of the collection - this describes how are test cases structured in file system, plus structures of test cases. This automatically helps to create order of the tests.

Normally, the order of test cases is alphabetical and is derived from collection structure. Although, end-user is able to re-define the order in cofiguration file. If this file is found in the beginning of the run, new order of the test cases is computed. For calculation of the order there will be probably some internal method.

```csharp
internal static IEnumerable<TestCase> ComputeTestCasesOrder(CollectionStructure structure);
```

Therefore, runner will work with two connected structures - **collection structure** and **order of test cases**. Structure of the collection will indicate hierarchical folder structure and references, while order will say in which order these pieces should be run.

Other than that, it will be possible to manipulate with the order of the test  cases during run-time. This will change only order of test cases, while collection structure remains the same. Such change can be triggered by end-user within `TeaPie` class, which will be delegated here. Therefore `CollectionRunner` will follow public API:

```csharp
public void NextTestCase(string testCaseName); // Determines which test case will run next
public void NextFolder(string folderName); // Determines which folder (containing test cases) will run next
public void SkipTestCase(string testCaseName); // Test case to be ignored/skipped
public void SkipTestCases(params string[] testCaseNames); // Test cases to be ignored/skipped
public void SkipFolder(string folderName); // Folder to be skipped at all
```

## Testing Context

### `TestingContext`

This class will hold all information about tests defined by end-user. It will know which tests are registered, ran, succeded and failed. Reporting service will take information from here.

End-user is able to write tests by using these public methods:

```csharp
public void Test(string testName, Action test); // Determined test method
public void Test(string testName, Func<Task> asyncTest); // Async test method
public void AddTestTheory(string theoryName, Action<Theory> theory); // Adds a test theory
```

Each call of these methods will have consequencies on `TestingContext` instance.

## HTTP Client

### `HttpClient`

Is class that enrich `System.Net.Http.HttpClient` class with customized functionality. Its main purpose will be send requests and then receive responses. This class will work with `ExecutionContext`.

### `ExecutionContext`

Contains all necessary information about currently proceeded test case. Which requests were ran and which responses were received. Enable end-user to work with all necessary properties of current request and response.

## Variables

### `Variable`

Class interpretation of variable - key and value.

### `VariablesCollection`

Will be extension for `Dictionary<string, Variable>` which will easify some work with variables. It will support `Contains<T>(string, T)`, `Get<T>(string, T)`, `Set<T>(string, T)` and `Remove(string)` methods for work with its variables.

Variables will be stored on each level separately. Accessing and adding variable(s) will be possible also on general level:

```csharp
// Variables
public VariablesCollection GlobalVariables { get; }
public VariablesCollection EnvironmentVariables { get; }
public VariablesCollection CollectionVariables { get; }
public VariablesCollection ScopeVariables { get; }
public VariablesCollection TestCaseVariables { get; }

// Variables methods
public bool ContainsVariable(string name); // Check if variable exist
public T GetVariable<T>(string name, T defaultValue = default); // Gets a variable, if not found, return default value
public void SetVariable<T>(string name, T value); // Sets a variable
public void RemoveVariable(string name); // Removes all variables by their prefix
public void RemoveVariables(string prefix); // Removes all variables by their prefix
```

## Logger

Main class will have read-only property of `ILogger` type (`Microsoft.Logging.Abstraction`), which will be implemented by default by `Serilog`. This will be accessible for end-user - this way user will be able to log everything necessary.

In the future, we can try to let user decide which implementation will be used.

## Reporting Service

### `ReportingService`

Is responsible for creating report for end-user, which will contain all information about execution of end-user-defined test cases and their tests. It will closely cooperate with `TestingContext` class.

## Application

### `Application`

Will encapsulate all segments above. In order to make it usable from outside (CLI application), **Builder design pattern** should be considered.
