# C# Scripting (.csx Files) Reference

Complete reference for writing C# scripts in TeaPie test cases.

## Scripting Basics

`.csx` files are **full C# with .NET runtime** but in scripting mode:

- **No class wrapper needed** - code runs directly at top level
- Can use all C# features: async/await, LINQ, generics, etc.
- NuGet packages available via `#nuget` directive
- Full access to .NET standard library and any referenced packages

## The `tp` Object API

The globally available `tp` object provides access to TeaPie execution context:

| Member | Description |
|--------|-------------|
| `tp.Request` | Current request being executed |
| `tp.Response` | Response from current request |
| `tp.Requests["name"]` | Access named requests from history |
| `tp.Responses["name"]` | Access named responses from history |
| `tp.Test(name, action)` | Define a test (sync) |
| `tp.Test(name, asyncAction)` | Define a test (async, use with `await`) |
| `tp.SetVariable(name, value, ...tags)` | Set variable |
| `tp.GetVariable<T>(name)` | Get variable |
| `tp.Logger` | ILogger for logging |

## Writing Tests

Tests are written using `tp.Test()` method. A test fails if an exception is thrown within its body.

### Synchronous Tests

```csharp
tp.Test("Response should have positive ID.", () =>
{
    dynamic body = tp.Response.GetBodyAsExpando();
    True(body.Id > 0);
});
```

### Asynchronous Tests

Use `await` for async operations:

```csharp
await tp.Test("Response should contain customer data.", async () =>
{
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    NotNull(response.Id);
    NotNull(response.Name);
});
```

### Extracting Variables

Store values from responses for use in subsequent requests:

```csharp
await tp.Test("Store created entity.", async () =>
{
    string responseBody = await tp.Response.Content.ReadAsStringAsync();
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    
    tp.SetVariable("EntityId", response.Id);
    tp.SetVariable("CreatedEntity", responseBody);
});
```

**Best practice:** Store entire response objects as JSON strings (not individual properties) for comparison with `JsonContains`. Delete temporary variables after use if they're only needed between two consecutive tests using `tp.RemoveVariable("VariableName")`.

### Conditional Test Execution

Skip tests conditionally:

```csharp
var shouldValidate = tp.GetVariable<bool>("ValidateEmail", true);
tp.Test("Email format should be valid", () =>
{
    var email = tp.Response.GetBodyAsExpando().Email;
    True(email.Contains("@"));
}, skipTest: !shouldValidate);
```

**Note:** For status code validation, use directives in `.http` files:

```http
## TEST-EXPECT-STATUS: [200, 201]
```

**Note:** When status code is validated via `## TEST-EXPECT-STATUS` directive in `.http` file, you don't need to check it again in test script. Only check status code in script if you need conditional logic based on status code.

**Example - checking status code in script (only when needed):**

```csharp
var statusCode = tp.Response.StatusCode;
Equal(System.Net.HttpStatusCode.NoContent, statusCode);
```

**For DELETE endpoints with 204 No Content:** If status code is validated by directive, test script is often not needed unless additional validations are required.

**Accessing status code from named responses (for validation error tests):**

```csharp
await tp.Test("Validation error should return 400.", async () =>
{
    var responseObj = tp.Responses["CreateProductInvalidRequest"];
    Equal(System.Net.HttpStatusCode.BadRequest, responseObj.StatusCode);
    
    if (responseObj.StatusCode == System.Net.HttpStatusCode.BadRequest)
    {
        dynamic response = await responseObj.GetBodyAsExpandoAsync();
        NotNull(response.errors);
    }
});
```

## Assertions (xUnit)

xUnit.Assert is globally imported - no `Assert.` prefix needed:

```csharp
Equal(expected, actual);
True(condition);
False(condition);
NotNull(value);
Null(value);
Contains(substring, text);
Fail("message");
...
```

## JSON Handling

**Decision Rules:**

1. **JSON Object** (`{...}`) → Always use `GetBodyAsExpandoAsync()`
2. **JSON Array** (`[...]`) → Use string checks OR deserialization:
   - **String checks**: Simple validations (structure, ID presence)
   - **Deserialization**: Complex validations (iteration, nested properties, counting)

**IMPORTANT:** `GetBodyAsExpandoAsync()` throws exception for arrays. Check response structure first.

### Getting Response Body as Expando

`CaseInsensitiveExpandoObject` provides case-insensitive property access:

```csharp
dynamic body = tp.Response.GetBodyAsExpando();
dynamic body = await tp.Response.GetBodyAsExpandoAsync();
```

### Getting Request Body as Expando

Access request body from current request:

```csharp
dynamic request = await tp.Request.GetBodyAsExpandoAsync();
var name = request.Name;
var price = request.Price;
```

Compare request and response data:

```csharp
await tp.Test("Response should match request data.", async () =>
{
    dynamic request = await tp.Request.GetBodyAsExpandoAsync();
    dynamic response = await tp.Response.GetBodyAsExpandoAsync();
    
    Equal(request.Name, response.Name);
    Equal(request.Price, response.Price);
});
```

**Note:** `CaseInsensitiveExpandoObject` provides case-insensitive property access. This is useful when comparing request (PascalCase) with response (camelCase):

```csharp
dynamic request = await tp.Request.GetBodyAsExpandoAsync();
dynamic response = await tp.Response.GetBodyAsExpandoAsync();

// These work even if request uses "Name" and response uses "name"
Equal(request.Name, response.name);
Equal(request.Name, response.Name);  // Both work
```

### Accessing Properties

```csharp
var id = body.Id;           // int
var name = body.Name;        // string
var price = body.Price;      // decimal (cast to double for comparison)
```

**Comparing decimal values:**

```csharp
// Cast to double for comparison
Equal(99.99, (double)response.Price);

// Or compare directly if types match
Equal(request.Price, response.Price);  // Works if both are dynamic decimals
```

### Handling JSON Arrays

`GetBodyAsExpandoAsync()` works only for JSON objects. For arrays, choose based on validation needs:

**Simple validations (string checks):**
```csharp
await tp.Test("Response should be an array.", async () =>
{
    string responseBody = await tp.Response.Content.ReadAsStringAsync();
    True(responseBody.TrimStart().StartsWith("["));
    True(responseBody.TrimEnd().EndsWith("]"));
});

await tp.Test("Created item should exist in list.", async () =>
{
    string responseBody = await tp.Response.Content.ReadAsStringAsync();
    int itemId = tp.GetVariable<int>("ItemId");
    True(responseBody.Contains($"\"id\":{itemId}") || responseBody.Contains($"\"id\": {itemId}"));
});
```

**Complex validations (deserialization):**
```csharp
await tp.Test("Response should contain multiple items.", async () =>
{
    string responseBody = await tp.Response.Content.ReadAsStringAsync();
    var items = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement[]>(responseBody);
    NotNull(items);
    True(items.Length > 0);

    // Access individual items
    var firstItem = items[0];
    True(firstItem.TryGetProperty("id", out var id));
});
```

### JSON String Extensions

Serialize objects to JSON:

```csharp
string json = someObject.ToJsonString();
```

### JSON Assertions

Verify that one JSON object contains another:

```csharp
// Basic usage
JsonContains(expectedJson, actualJson);

// Exclude server-generated properties from comparison
JsonContains(responseBody, requestBody, "id", "createdAt");
```

See [Test Design Best Practices](test-design.md) for recommended usage patterns.

**JsonContains Case-Sensitivity:** `JsonContains()` compares JSON strings directly, so property names must match exactly. When request uses PascalCase (`Name`, `Description`) and response uses camelCase (`name`, `description`) - common with ASP.NET Core default serialization - use `CaseInsensitiveExpandoObject` or JsonElement for comparison instead. See [JsonContains Limitations](test-design.md#jsoncontains-limitations) for details and alternatives.

### Request Variables in .http Files

Access JSON properties from previous requests using JSONPath syntax:

```http
### Get Car
GET {{ApiBaseUrl}}{{ApiCarsSection}}/{{AddCarRequest.response.body.$.Id}}
```

## Code Style

**Do not add comments to test scripts.** Test names should be descriptive and self-documenting. Keep code clean without explanatory comments.
