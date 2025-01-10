## Installation
In order to make application working, you have to run these commands:

`cd "..\src\TeaPie.DotnetTool"` - get to the project directory

`dotnet pack -c Release` - packs its `Release` version

`copy ".\bin\Release\TeaPie.Tool.1.0.0.nupkg" "path\to\your\local\nuget\feed"` - copy `.nupkg` file to your local NuGet feed`*`. _Adjust to current version of the tool if needed._

`dotnet tool install -g TeaPie.Tool` - **install** tool to your computer

Now, tool should be **ready-to-use** by command `teapie`.

------------------------
`*` if you don't have your local NuGet feed, you can setup it like this:

`mkdir "path/to/your/new/local/feed/directory"` - creates directory

`dotnet nuget add source "path/to/your/local/feed" --name NameOfYourLocalFeed` - adds new local feed

## Usage
To start, **create your first test-case** by using:

`teapie generate <test-case name> [path] [-i|--init|--pre-request] [-t|--test|--post-response]`

This command generates 3 files on given path (or in current directory, if not given) by default: 
- **pre-request script** (`*-init.csx`), 
- **request file** (`*-req.http`),
- **post-response** (`*-test.csx`)

You can set options `-i` and `-t` to `FALSE`, if you want to omit generation of either pre-request script or post-response script.

### Pre-request script
In **pre-request script** _set your variables and initialize_ what is needed before sending request. You can use `#nuget Package.Id, Version` directive for installation **NuGet packages** for further use. Although **NuGet package is installed globally** across all scripts, you have to use `using` keyword in each script in order to use its functionality.

To access **test runner context** `TeaPie` instance with name `tp` is accessible everywhere in the scripts.
```csharp
tp.SetVariable("CurrentTime", DateTime.UtcNow);
```

If you have some script, which already does your initialization, you can reference it by using `#load "path\to\your\script.csx"` directive. Parser is configured in way, that you can use either **absolute or relative path**. When using relative path, parent folder of script is used as default path. Load directive enables you to use functions of referenced script, although it **doesn't run this script.**

### Request file
**Request file** can have either **one or more HTTP requests**. In order to **separate them**, use `###` comment line between two requests. For better requests manipulation, you can name them - simply by adding metadata line `# @name RequestName` before request definition.

All variables from environment and those set in scripts can be accessed by `{{variableName}}` notation. It will be **replaced by its string value** - so variable can contain also reference types which should have overriden `ToString()` method.

You can approach content of named requests already in request file by using **request variables**. Such variable consists of 4 parts: `{{requestName.(request|response).(body|headers).(*|JPath|XPath)}}`. This means, that you have unlimited access to all headers and body content of your named requests.

### Post-response script
This file should contain **determined tests** defined by you. **Test** is considered as **failed**, if **exception is thrown** from the code you write as test body. This follows common practices of other testing environments. Advantage of this approach is, that you **can use any assertion library** (if it is referenced as NuGet already) for your tests.

This is example of simple test:
```csharp
tp.Test("Status code should be 201.", () => 
{
    var statusCode = tp.Responses["RequestName"].StatusCode;
    ((int)statusCode).Should().Be(201);
});
```

Notice, how easily can you access response object of any named request. This works exactly the same with request objects in `tp.Requests` dictionary. These dictionaries are extra useful, if you have multiple requests within one request file. 

In the case of single request in request file, you can approach it even without naming it - by `tp.Request`, respectively `tp.Response` properties. These objects are of `HttpRequestMessage` and `HttpResponseMessage` type, which are enriched by useful methods `GetBody` and `GetBodyAsync`, which return string representation of body content.

Since significant part of HTTP traffic handle `json/application` payloads, **extension method** for `string` `ToJson()` was added to easify access to its properties.
```csharp
tp.Test("Identificator should be positive integer", () =>
{
    var responseBody = tp.Response.GetBody();

})
```  
