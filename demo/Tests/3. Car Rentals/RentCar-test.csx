#load "./RentCar-init.csx"

using Mapster;

var car = new Car("Toyota", "RAV4", 2022);

tp.Test("Car should be rented successfully.", () =>
{
    tp.Responses["RentCarRequest"].StatusCode().Should().Be(201);
    tp.Response.StatusCode().Should().Be(200);
});

tp.Test($"Rented car should be '{car}'.", () =>
{
    var carResponse = tp.Response.GetBody().ToJson();
    var retrievedCar = carResponse.Adapt<Car>();

    retrievedCar.Brand.Should().Be(car.Brand);
    retrievedCar.Model.Should().Be(car.Model);
    retrievedCar.Year.Should().Be(car.Year);
});
