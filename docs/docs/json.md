# JSON Handling

Since a large portion of HTTP traffic relies on JSON contracts, this tool provides convenient methods for working with JSON bodies.

## Request variables

Request variables are used within `.http` files to access data from previous requests and responses. You can access JSON properties using `JPath` syntax, like this:

```http
# Example: Extract the `id` property from the JSON response of the `NewBook` request
{{NewBook.response.body.$.id}}
```

## JSON to Expando

One of the most convenient ways to work with JSON in C# scripts is by converting it into a case-insensitive `ExpandoObject`:

```csharp
// Parses a JSON string into a case-insensitive expando object.
// Properties are accessible like regular class properties.
public static CaseInsensitiveExpandoObject ToExpando(this string jsonText)
```

This transforms a JSON string into a `dynamic` object whose properties can be accessed just like any other object's:

```csharp
// Use 'dynamic' (not 'var') to work with expando objects.
dynamic customer = customerJson.ToExpando();
tp.SetVariable("NewCustomerId", customer.id); // Case-insensitive property access
```

Since the body needs to be retrieved from a request or response and then converted into an expando object, there’s a shortcut method that combines both steps:

```csharp
dynamic customer = await tp.Request.GetBodyAsExpandoAsync();
```

## JSON string extensions

If you have a JSON object serialized as a string, you can use the following extension methods:

```csharp
// Merges two JSON strings. 'otherJson' will overwrite or add properties to 'baseJson'.
public static string CombineWithJson(this string baseJson, string otherJson);

// Adds a new property with the specified name and value to the JSON string.
public static string AddJsonProperty<T>(this string jsonText, string propertyName, T propertyValue);

// Removes the specified property from the JSON string.
public static string RemoveJsonProperty(this string jsonText, string propertyName);

// Converts the JSON string to an object of type 'TResult'.
public static TResult? To<TResult>(this string jsonText);

// Parses the string into a JObject using Newtonsoft.Json.
public static JObject ToJson(this string text);
```

To serialize any object into a JSON string, you can use:

```csharp
// Serializes the object into a JSON string.
public static string ToJsonString(this object obj);
```

## Assertions

You can verify that one JSON object contains another using the following assertion method. Both parameters must be JSON strings:

```csharp
JsonContains(tp.Response.GetBody(), expectedPartnerWithPriceLevel);
```
