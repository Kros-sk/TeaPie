# How to Write Tests

The test should be written either in the code - within **post-response scripts** or by using **test dirctives** within **request files**.

## Testing in Post-Response Script

Writing tests in post-response script can be done by extension methods over `tp` instance.

A test is considered **failed** if an exception is thrown within the test body, following standard testing framework practices. This approach allows you to **use any assertion library** referenced via NuGet.

> 💁‍♂️ However, the **natively supported assertion library** is `Xunit.Assert`, which is statically imported in all script files. This means you don't need the `Assert.` prefix to access its methods.

> ⚠️ `.csx` scripts **must not contain code that accesses or processes HTTP responses outside of `tp.Test` methods**. Since the script is executed before all HTTP requests, any response‑dependent logic placed outside `tp.Test` methods **may cause errors**.

### Example Test

```csharp
tp.Test("Status code should be 201.", () =>
{
    var statusCode = tp.Response.StatusCode();
    Equal(201, statusCode);
});
```

### Asynchronous Tests

Since asynchronous operations are gaining on popularity, `TeaPie` support asynchronous tests.

```csharp
await tp.Test($"Newly added car should have '{brand}' brand.", async () =>
{
    var newCar = tp.GetVariable<string>("NewCar");
    dynamic obj = newCar.ToExpando();

    dynamic responseJson = await tp.Responses["GetNewCarRequest"].GetBodyAsExpandoAsync();
    Equal(obj.Brand, responseJson.brand);
});
```

### Skipping Tests

During development or debugging, you may need to skip certain tests. To do this, set the optional `skipTest` parameter to `true`:

```csharp
tp.Test("Status code should be 201.", () =>
{
    var statusCode = tp.Responses["CreateCarRequest"].StatusCode();
    Equal(201, statusCode);
}, skipTest: true); // Skip this test
```

## Testing in Request File

Some testing scenarios tend to repeat - for that reason, you can use **test directives**, which handle common scenarios. TeaPie already support some of these, but you can register your very own directives, too.

The actual **test** coming from directive is **scheduled right after HTTP request execution** to which it was assigned to.

### Supported Directives

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

Full list of (not only) test directives is [here](directives.md).

### Custom Testing Directives  

Each project may have **repeating test scenarios** that require a **customized approach**.  
TeaPie allows you to **dynamically register new testing directives** that can be used in `.http` files.

#### Registering a Custom Testing Directive  

Before using a **custom testing directive**, it must be **registered in a script** that executes **before its first usage**.  

**Required elements:**

| **Component** | **Description** |
|--------------|----------------|
| **Directive Name** | The name of the directive, which will be prepended by the `TEST-` prefix. |
| **Regular Expression Pattern** | Defines how the directive should be parsed. Use [TestDirectivePatternBuilder](xref:TeaPie.Testing.TestDirectivePatternBuilder) to simplify pattern creation. |
| **Test Name Generator** | A function that takes directive parameters and generates a test name assigned to all tests fired by the directive. |
| **Testing Function** | A function that performs the test, receiving an `HttpResponseMessage` and a `Dictionary<string, string>` of parameters. |

Registering a new testing directive example:

```csharp
tp.RegisterTestDirective(
    "SUCCESSFUL-STATUS",
    TestDirectivePatternBuilder
        .Create("SUCCESSFUL-STATUS")
        .AddBooleanParameter("MyBool")
        .Build(),
    (parameters) =>
    {
        var negation = bool.Parse(parameters["MyBool"]) ? string.Empty : "NOT ";
        return $"Response status code should {negation}be successful.";
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

#### Using a Custom Testing Directive  

Once registered, the directive can be used in a `.http` file as follows:  

```http
## TEST-SUCCESSFUL-STATUS: True
PUT {{ApiBaseUrl}}{{ApiCarsSection}}/{{AddCarRequest.request.body.$.Id}}
Content-Type: {{AddCarRequest.request.headers.Content-Type}}

...
```
