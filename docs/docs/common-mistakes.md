# Common Mistakes

This section lists and explains common mistakes that may occur during the test development process. To save time and avoid frustration, review these points when troubleshooting.

## 'CaseInsensitiveExpandoObject' does not contain a definition for 'propertyName'

**Reason:**
This error likely occurs because the result of `ToExpando()`, `GetBodyAsExpando()`, or `GetBodyAsExpandoAsync()` was not declared as a `dynamic` variable.

**Solution:**
Declare the variable holding the expando object **explicitly** as `dynamic`:

```csharp
// Incorrect:
var customer = await tp.Request.GetBodyAsExpandoAsync();

// Correct:
dynamic customer = await tp.Request.GetBodyAsExpandoAsync();
