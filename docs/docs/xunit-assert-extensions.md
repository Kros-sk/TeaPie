# Xunit.Assert Extensions

TeaPie extends the default `Xunit.Assert` with additional assertion methods to make your tests more expressive and provide better failure messages.

These methods are added directly to `Xunit.Assert` using **C# 14 extension types**, so they can be called with the standard `Assert.` prefix — or without it, since `Xunit.Assert` is **statically imported** in all script files.

> 💡 Following the xUnit convention, the **first argument is the expected boundary** (limit/threshold) and the **second argument is the actual value** being verified.

## Comparison Assertions

These methods work for **any type implementing `IComparable<T>`** (e.g., `int`, `long`, `DateTime`, `string`, etc.).  
For floating-point types (`double` and `float`), additional overloads with an **epsilon** parameter are provided to account for floating-point imprecision.

### GreaterThan

Verifies that `value` is greater than `limit`.

```csharp
Assert.GreaterThan(limit, value);
Assert.GreaterThan(limit, value, epsilon); // For double and float
```

**Example:**

```csharp
tp.Test("Response time should be acceptable.", () =>
{
    var responseTimeMs = tp.Response.Headers.Age?.TotalMilliseconds ?? 0;
    Assert.GreaterThan(0, responseTimeMs); // More readable than: True(responseTimeMs > 0)
});
```

### GreaterThanOrEqual

Verifies that `value` is greater than or equal to `limit`.

```csharp
Assert.GreaterThanOrEqual(limit, value);
Assert.GreaterThanOrEqual(limit, value, epsilon); // For double and float
```

### LessThan

Verifies that `value` is less than `limit`.

```csharp
Assert.LessThan(limit, value);
Assert.LessThan(limit, value, epsilon); // For double and float
```

**Example:**

```csharp
tp.Test("Status code should be a client error.", () =>
{
    var statusCode = tp.Response.StatusCode();
    Assert.GreaterThanOrEqual(400, statusCode);
    Assert.LessThan(500, statusCode);
});
```

### LessThanOrEqual

Verifies that `value` is less than or equal to `limit`.

```csharp
Assert.LessThanOrEqual(limit, value);
Assert.LessThanOrEqual(limit, value, epsilon); // For double and float
```

### Epsilon Parameter

When comparing `double` or `float` values, an **epsilon** parameter can be specified to define the maximum allowed difference for two values to be considered equal. This is useful when dealing with floating-point arithmetic imprecision.

```csharp
tp.Test("Price should be within acceptable range.", () =>
{
    var price = tp.GetVariable<double>("ItemPrice");
    Assert.GreaterThan(0.0, price, 0.001);    // price must be > 0.001
    Assert.LessThan(1000.0, price, 0.001);    // price must be < 999.999
});
```

## Null or Empty Assertions

These methods apply to both **strings** and **collections**.

### NullOrEmpty

Verifies that a string or collection is `null` or empty.

```csharp
Assert.NullOrEmpty(value);      // string
Assert.NullOrEmpty(collection); // IEnumerable<T>
```

**Example:**

```csharp
tp.Test("Error list should be empty.", () =>
{
    var errors = tp.GetVariable<string[]>("ValidationErrors");
    Assert.NullOrEmpty(errors);
});
```

### NotNullOrEmpty

Verifies that a string or collection is **not** `null` and **not** empty.

```csharp
Assert.NotNullOrEmpty(value);      // string
Assert.NotNullOrEmpty(collection); // IEnumerable<T>
```

**Example:**

```csharp
await tp.Test("Response body should not be empty.", async () =>
{
    var body = await tp.Response.GetBodyAsStringAsync();
    Assert.NotNullOrEmpty(body);
});
```

## JsonContains

Verifies that a JSON string contains another JSON object. Optionally, specific properties can be excluded from the comparison.

```csharp
JsonContains(container, contained);
JsonContains(container, contained, "propertyToIgnore", "anotherProperty");
```

**Example:**

```csharp
await tp.Test("Response should contain the expected customer data.", async () =>
{
    var responseBody = await tp.Response.GetBodyAsStringAsync();
    var expected = """{ "name": "Alice", "age": 30 }""";
    JsonContains(responseBody, expected);
});
```
