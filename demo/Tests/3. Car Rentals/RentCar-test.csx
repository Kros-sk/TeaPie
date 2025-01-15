#load "./RentCar-init.csx"

using Mapster;

var car = new Car("Toyota", "RAV4", 2022);

tp.Test("Car should be rented successfully.", () =>
{
    Equal(tp.Responses["RentCarRequest"].StatusCode(), 201);
    Equal(tp.Response.StatusCode(), 200);
});

tp.Test($"Rented car should be '{car}'.", () =>
{
    var carResponse = tp.Response.GetBody().ToJson();
    var retrievedCar = carResponse.Adapt<Car>();

    Equal(retrievedCar.Brand, car.Brand);
    Equal(retrievedCar.Model, car.Model);
    Equal(retrievedCar.Year, car.Year);
});
