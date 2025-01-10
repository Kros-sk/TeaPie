using FluentAssertions;

tp.Test("Status code of car addition should be 201. (Created)", () =>
    {
        var statusCode = tp.Responses["AddCarRequest"].StatusCode;
        ((int)statusCode).Should().Be(201);
    }
);

tp.Test("Identificators of added and then retrieved car should match.", () =>
    {
        var requestJson = tp.Requests["AddCarRequest"].GetBody().ToJson();
        var responseJson = tp.Responses["GetNewCarRequest"].GetBody().ToJson();
        requestJson["Id"].ToString().Should().Be(responseJson["Id"].ToString());
    }
);