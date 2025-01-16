// Reference of multiple scripts is also allowed.
#load "../ClearVariables.csx"
#load "./RentCar-init.csx"

// Use a NuGet package which was downloaded in another script.
using Mapster;

// Instantiate a record/class/structure defined in the referenced script.
var car = new Car("Toyota", "RAV4", 2022);

tp.Test("Car should be rented successfully.", () =>
{
    // Access a named response.
    Equal(tp.Responses["RentCarRequest"].StatusCode(), 201);
    // Access the last executed response.
    Equal(tp.Response.StatusCode(), 200);
});

// Interpolated strings resolve correctly (Car overrides the ToString() method).
tp.Test($"Rented car should be '{car}'.", () =>
{
    var carResponse = tp.Response.GetBody().ToJson();
    var retrievedCar = carResponse.Adapt<Car>();

    Equal(retrievedCar.Brand, car.Brand);
    Equal(retrievedCar.Model, car.Model);
    Equal(retrievedCar.Year, car.Year);
});

ClearVariables();
