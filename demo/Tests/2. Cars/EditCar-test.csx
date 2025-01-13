tp.Test("Status code of car retrieval should be 200. (OK)", () =>
    {
        var statusCode = tp.Responses["GetEditedCarRequest"].StatusCode;
        ((int)statusCode).Should().Be(200);
    }
);

tp.Test("Engine type and people capacity should be edited.", () =>
    {
        var oldCarJson = tp.Requests["AddCarRequest"].GetBody().ToJson();
        var newCarJson = tp.Responses["GetEditedCarRequest"].GetBody().ToJson();
        oldCarJson["EngineType"].ToString().Should().Be(newCarJson["EngineType"].ToString());
        oldCarJson["PeopleCapacity"].ToString().Should().Be(newCarJson["PeopleCapacity"].ToString());
    }
);
