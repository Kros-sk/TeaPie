# Pre-request Script

|   |   |
|----------------------|----------------|
| **Definition**       | The first `.csx` script executed within a test case. |
| **Naming Convention** | `<test-case-name>-init.csx` |
| **Purpose**         | Initialization of data and variables before executing any HTTP request. |
| **Example Usage**         | [Demo Pre-Request Script](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/002-Cars/001-Add-Car-init.csx) |

## Features

### Variables

You can access the **test runner context** using the globally available `tp` identifier for various purposes. One of them is variables setting/getting:

```csharp
tp.SetVariable("TimeOfExecution", DateTime.UtcNow);
...
var time = tp.GetVariable("TimeOfExecution");
```

### Directives

#### Load Directive

`#load` directive for **referencing another scripts**

You can provide either an *absolute* or a *relative path*.

**IMPORTANT:** Referenced script is **automatically executed**. For this reason, rather encapsulate logic in methods, to prevent unwanted execution.

```csharp
#load "path\to\your\script.csx"
```

>💁‍♂️ When using relative paths, the parent folder of the current script serves as the starting point.

#### NuGet Directive

`#nuget` directive to install **NuGet packages**:

```csharp
#nuget "AutoBogus, 2.13.1"
```

>💁‍♂️ Even though NuGet packages are installed globally across all scripts, you must use the `using` directive to access them in your scripts.

### Data Generation

As you can see in the provided example [Demo Pre-Request Script](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/002-Cars/001-Add-Car-init.csx), there is method which generates the data - `GenerateCar()`, which is defined in script [GenerateNewCar.csx](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/002-Cars/Definitions/GenerateNewCar.csx). The foundation of the data generation is in the class [CarFaker.csx](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/002-Cars/Definitions/CarFaker.csx).

The example for data generation uses NuGet packagte `AutoBogus`.
