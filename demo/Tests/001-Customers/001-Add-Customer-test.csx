// Native support for asynchronous tests
await tp.Test("Customer should be created successfully.", async () =>
{
    // Assertions use XUnit.Assert by default, but you can use any assertion library.
    // 'Assert.' qualifier is optional with XUnit. StatusCode() returns an int => no casting needed.
    Equal(201, tp.Response.StatusCode()); // Equivalent to Assert.Equal(201, (int)tp.Response.StatusCode);

    // For easier access to content body, there are few handy methods in sync and async form.
    // To use dynamic expando objects, explicitly declare them as 'dynamic' (not 'var').
    dynamic customer = await tp.Request.GetBodyAsExpandoAsync();

    // Access expando properties dynamically and case-insensitively.
    // When storing to variables, type has to be explicitly written.
    tp.SetVariable("NewCustomerId", customer.id); // Equivalent to tp.CollectionVariables.Set("NewCustomerId", customer.Id);

    // Alternatively, use JSON parsing:
    // var body = tp.Request.GetBody().ToJson();
    // var id = (long)body["Id"];
});

await tp.Test("Customer data should be valid.", async () =>
{
    dynamic customer = await tp.Request.GetBodyAsExpandoAsync();

    // TeaPie extends Xunit.Assert with additional assertion methods for more expressive tests.
    // Use NotNullOrEmpty to verify that a string value is present.
    NotNullOrEmpty((string)customer.firstName);
    NotNullOrEmpty((string)customer.lastName);
    NotNullOrEmpty((string)customer.email);

    // Use GreaterThan to verify that a numeric value exceeds a threshold.
    GreaterThan((long)customer.id, 0L);
});
