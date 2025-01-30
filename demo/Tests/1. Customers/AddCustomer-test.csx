// Native support for asynchronous tests
await tp.Test("Customer should be created successfully.", async () =>
{
    // Assertions use XUnit.Assert by default, but you can use any assertion library.
    // 'Assert.' qualifier is optional with XUnit. StatusCode() returns an int => no casting needed.
    Equal(201, tp.Response.StatusCode()); // Equivalent to Assert.Equal((int)tp.Response.StatusCode, 201);

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
