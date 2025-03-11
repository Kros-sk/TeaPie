# Post-Response Script

|   |   |
|----------------------|----------------|
| **Definition**       | `.csx` script to be executed after all HTTP requests within test case. |
| **Naming Convention** | `<test-case-name>-test.csx` |
| **Purpose**         | Testing given response(s) and tear-down of data. |
| **Example Usage**         | [Simple Script](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/1.%20Customers/AddCustomer-test.csx), [Manipulation with Body](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/2.%20Cars/AddCar-test.csx), [Another Example](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/2.%20Cars/EditCar-test.csx), [Advanced Script](https://github.com/Kros-sk/TeaPie/blob/master/demo/Tests/3.%20Car%20Rentals/RentCar-test.csx) |

## Features

### Testing

The main role of **post-response script** is to **validate responses** and **define tests**.

To learn more about how to write a test, visit [this page](../how-to-write-tests.md).

### Accessing Requests and Responses

- For **single requests** or the **most recently executed request**, use `tp.Request` and `tp.Response`.
- For **multiple requests** in a `.http` file, use `tp.Requests` and `tp.Responses` to access named requests and responses.

#### Working with Body Content

Both `HttpRequestMessage` and `HttpResponseMessage` objects include convenient methods for handling body content:

- `GetBody()` / `GetBodyAsync()` - Retrieves the body as a `string`.
- `GetBody<TResult>()` / `GetBodyAsync<TResult>()` - Deserializes the JSON body into an object of type `TResult`.
- `GetBodyAsExpando()` / `GetBodyAsExpandoAsync()` - Retrieves the body as a **case-insensitive `dynamic` expando object**, making property access easier.
  - **IMPORTANT**: To use an expando object correctly, **explicitly declare containing variable** as `dynamic`.

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

#### Status Code Handling

The response object includes a `StatusCode()` method that simplifies status code handling by returning its **integer** value.
