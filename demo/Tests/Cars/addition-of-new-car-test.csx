#nuget "FluentAssertions, 6.12.1"

using FluentAssertions;

tp.Test("Status code of car addition should be 201 (Created)",
    tp.Responses["AddCarRequest"].StatusCode.Should().Be(201);
);

tp.Test("Status code of getting deleted car should be 404 (NotFound)",

    tp.Responses["GetDeletedCarRequest"].Should().Be();
);

tp.SetVariable("Cars-NewCar-Id", tp.Context.Respose.Body["Id"]);
tp.NextTestCase("Get All Cars");

