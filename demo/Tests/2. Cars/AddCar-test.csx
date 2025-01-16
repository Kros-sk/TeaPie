tp.Test("Status code of car addition should be 201 (Created).", () => {
    // Access named responses using their name.
    var statusCode = tp.Responses["AddCarRequest"].StatusCode();
    Equal(statusCode, 201);
});

// Asynchronous tests are supported.
await tp.Test("Newly added car should have 'Toyota' brand.", async () => {
    var responseBody = await tp.Responses["GetNewCarRequest"].GetBodyAsync();
    dynamic responseJson = responseBody.ToJsonExpando();

    // Access JSON properties case-insensitively.
    Equal(responseJson.brand, "Toyota");
});

await tp.Test("Identifiers of added and retrieved cars should match.", async () => {
    // Access named requests like responses.
    // Use asynchronous body retrieval (recommended).
    var requestBody = await tp.Requests["AddCarRequest"].GetBodyAsync();
    dynamic requestJson = requestBody.ToJsonExpando();

    var responseBody = await tp.Responses["GetNewCarRequest"].GetBodyAsync();
    dynamic responseJson = responseBody.ToJsonExpando();

    Equal(requestJson.Id, responseJson.Id);

    // Each variable can have none or multiple tags ('cars', 'ids' in this case).
    tp.CollectionVariables.Set("NewCarId", requestJson.Id, "cars", "ids");
});

