tp.Test("Status code of car addition should be 201. (Created)", () => {
    var statusCode = tp.Responses["AddCarRequest"].StatusCode();
    statusCode.Should().Be(201);
});

await tp.Test("Newly added car should have 'Toyota' brand.", async () => {
    var responseBody = await tp.Responses["GetNewCarRequest"].GetBodyAsync();
    dynamic responseJson = responseBody.ToJsonExpando();

    // To access json properties like this, variable has to be explicitly 'dynamic' type
    StringShould(responseJson.brand).Be("Toyota");
});

await tp.Test("Identificators of added and then retrieved car should match.", async () => {
    var requestBody = await tp.Requests["AddCarRequest"].GetBodyAsync();
    dynamic requestJson = requestBody.ToJsonExpando();

    var responseBody = await tp.Responses["GetNewCarRequest"].GetBodyAsync();
    dynamic responseJson = responseBody.ToJsonExpando();

    LongShould(requestJson.Id).Be(responseJson.Id);
    tp.CollectionVariables.Set("NewCarId", requestJson.Id);
});
