tp.Test("Status code of car retrieval should be 200 (OK)", () => {
    var statusCode = tp.Responses["GetEditedCarRequest"].StatusCode();
    Equal(statusCode, 200);
});

tp.Test("Engine type and people capacity should match after edit.", () => {
    const string comparedField1 = "EngineType";
    const string comparedField2 = "PeopleCapacity";

    // Alternative approach of access to JSON objects.
    var oldCarJson = tp.Requests["AddCarRequest"].GetBody().ToJson();
    var newCarJson = tp.Responses["GetEditedCarRequest"].GetBody().ToJson();

    // This approach allows dynamic access to fields by their names which are resolved during run-time.
    Equal(oldCarJson[comparedField1], newCarJson[comparedField1]);
    Equal(oldCarJson[comparedField2], newCarJson[comparedField2]);
});
