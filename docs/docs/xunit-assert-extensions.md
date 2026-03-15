# Xunit.Assert Extensions

TeaPie extends the default `Xunit.Assert` with additional assertion methods to make your tests more expressive and provide better failure messages.

Since `Xunit.Assert` is **statically imported** in all script files, you can use these methods directly — **no prefix needed**.

## Comparison Assertions

These methods work for **any type implementing `IComparable<T>`** (e.g., `int`, `long`, `DateTime`, `string`, etc.).  
For floating-point types (`double` and `float`), additional overloads with an **epsilon** parameter are provided to account for floating-point imprecision.

### GreaterThan

Verifies that `value` is greater than `limit`.

```csharp
GreaterThan(value, limit);
GreaterThan(value, limit, epsilon); // For double and float
```

**Example:**

```csharp
tp.Test("Response time should be acceptable.", () =>
{
    var responseTimeMs = tp.Response.Headers.Age?.TotalMilliseconds ?? 0;
    GreaterThan(responseTimeMs, 0); // More readable than: True(responseTimeMs > 0)
});
```

### GreaterThanOrEqual

Verifies that `value` is greater than or equal to `limit`.

```csharp
GreaterThanOrEqual(value, limit);
GreaterThanOrEqual(value, limit, epsilon); // For double and float
```

### LessThan

Verifies that `value` is less than `limit`.

```csharp
LessThan(value, limit);
LessThan(value, limit, epsilon); // For double and float
```

**Example:**

```csharp
tp.Test("Status code should be a client error.", () =>
{
    var statusCode = tp.Response.StatusCode();
    GreaterThanOrEqual(statusCode, 400);
    LessThan(statusCode, 500);
});
```

### LessThanOrEqual

Verifies that `value` is less than or equal to `limit`.

```csharp
LessThanOrEqual(value, limit);
LessThanOrEqual(value, limit, epsilon); // For double and float
```

### Epsilon Parameter

When comparing `double` or `float` values, an **epsilon** parameter can be specified to define the maximum allowed difference for two values to be considered equal. This is useful when dealing with floating-point arithmetic imprecision.

```csharp
tp.Test("Price should be within acceptable range.", () =>
{
    var price = tp.GetVariable<double>("ItemPrice");
    GreaterThan(price, 0.0, 0.001);   // price must be > 0.001
    LessThan(price, 1000.0, 0.001);   // price must be < 999.999
});
```

## Null or Empty Assertions

These methods apply to both **strings** and **collections**.

### NullOrEmpty

Verifies that a string or collection is `null` or empty.

```csharp
NullOrEmpty(value);      // string
NullOrEmpty(collection); // IEnumerable<T>
```

**Example:**

```csharp
tp.Test("Error list should be empty.", () =>
{
    var errors = tp.GetVariable<string[]>("ValidationErrors");
    NullOrEmpty(errors);
});
```

### NotNullOrEmpty

Verifies that a string or collection is **not** `null` and **not** empty.

```csharp
NotNullOrEmpty(value);      // string
NotNullOrEmpty(collection); // IEnumerable<T>
```

**Example:**

```csharp
await tp.Test("Response body should not be empty.", async () =>
{
    var body = await tp.Response.GetBodyAsStringAsync();
    NotNullOrEmpty(body);
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
