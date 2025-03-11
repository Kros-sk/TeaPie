# How to Write Tests

The test should be written either in the code - within **post-response scripts** or by using **test dirctives** within **request files**.

## Testing in Post-Response Script

Writing tests in post-response script can be done by extension methods over `tp` instance.

A test is considered **failed** if an exception is thrown within the test body, following standard testing framework practices. This approach allows you to **use any assertion library** referenced via NuGet.

> 💁‍♂️ However, the **natively supported assertion library** is `Xunit.Assert`, which is statically imported in all script files. This means you don't need the `Assert.` prefix to access its methods.

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
    var body = tp.GetVariable<string>("NewCar");
    dynamic obj = body.ToExpando();

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
}, true); // Skip this test
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
