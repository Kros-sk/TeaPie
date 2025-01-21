tp.Test("Customer should be created successfully.", () => {
    // Assertions use XUnit.Assert by default, but you can use any assertion library.
    // 'Assert.' qualifier is optional with XUnit. StatusCode() returns an int => no casting needed.
    Equal(tp.Response.StatusCode(), 201); // Equivalent to Assert.Equal((int)tp.Response.StatusCode, 201);

    // Use GetBody() to get the request/response body as a string. Returns empty string if the body is null.
    string body = tp.Request.GetBody(); // Equivalent to await tp.Request.Content?.ReadAsStringAsync() ?? string.Empty;

    // To use dynamic expando objects, explicitly declare them as 'dynamic' (not 'var').
    dynamic customer = body.ToJsonExpando();

    // Access expando properties dynamically and store them in variables.
    tp.SetVariable("NewCustomerId", customer.Id); // Equivalent to tp.CollectionVariables.Set("NewCustomerId", customer.Id);

    // Alternatively, use JSON parsing:
    // var body = tp.Request.GetBody().ToJson();
    // var id = (long)body["Id"];
});
