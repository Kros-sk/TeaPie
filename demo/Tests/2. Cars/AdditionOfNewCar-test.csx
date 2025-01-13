tp.Test("Status code of car addition should be 201. (Created)", () =>
    {
        var statusCode = tp.Responses["AddCarRequest"].StatusCode();
        statusCode.Should().Be(201);
    }
);

tp.Test("Identificators of added and then retrieved car should match.", async () =>
    {
        var requestBody = await tp.Requests["AddCarRequest"].GetBodyAsync();
        var requestJson = requestBody.ToJson();

        var responseBody = await tp.Responses["GetNewCarRequest"].GetBodyAsync();
        var responseJson = responseBody.ToJson();

        requestJson["Id"].ToString().Should().Be(responseJson["Id"].ToString());
    }
);
