# Post-Response Script

|   |   |
|----------------------|----------------|
| **Definition**       | `.csx` script to be executed after all HTTP requests within test case. |
| **Naming Convention** | `<test-case-name>-test.csx` |
| **Purpose**         | Testing given response(s) and tear-down of data. |
| **Example Usage**         | [Simple Script](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/1.%20Customers/AddCustomer-test.csx), [Manipulation with Body](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/2.%20Cars/AddCar-test.csx), [Another Example](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/2.%20Cars/EditCar-test.csx), [Advanced Script](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/3.%20Car%20Rentals/RentCar-test.csx) |

## Features

### Testing

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
