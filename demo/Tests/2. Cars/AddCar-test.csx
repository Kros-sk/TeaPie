tp.Test("Status code of car addition should be 201 (Created).", () => {
    // Access named responses using their names.
    var statusCode = tp.Responses["AddCarRequest"].StatusCode();
    Equal(statusCode, 201);
});

// Approach to string variable with the name "NewCar".
var body = tp.GetVariable<string>("NewCar");
dynamic obj = body.ToExpando();
var brand = obj.Brand;

await tp.Test($"Newly added car should have '{brand}' brand.", async () => {
    dynamic responseJson = await tp.Responses["GetNewCarRequest"].GetBodyAsExpandoAsync();

    // Access JSON properties case-insensitively.
    Equal(responseJson.brand, brand);
});

await tp.Test("Identifiers of added and retrieved cars should match.", async () => {
    // Access named requests in the same way as responses.
    // Use an asynchronous body retrieval (recommended).
    dynamic requestJson = await tp.Requests["AddCarRequest"].GetBodyAsExpandoAsync();
    dynamic responseJson = await tp.Responses["GetNewCarRequest"].GetBodyAsExpandoAsync();

    Equal(requestJson.Id, responseJson.Id);

    // Each variable can have none or multiple tags ('cars', 'ids' in this case).
    tp.SetVariable("NewCarId", requestJson.Id, "cars", "ids");
});

