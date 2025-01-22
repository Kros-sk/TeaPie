// Reference of multiple scripts is also allowed.
#load "../ClearVariables.csx"
#load "../2. Cars/Definitions/Car.csx"

tp.Test("Car should be rented successfully.", () =>
{
    // Access a named response.
    Equal(tp.Responses["RentCarRequest"].StatusCode(), 201);
    // Access the last executed response.
    Equal(tp.Response.StatusCode(), 200);
});

// If you have variable in JSON string, it can be easily converted to reference type, by using 'To<TResult>()' method.
var car = tp.GetVariable<string>("NewCarBody").To<Car>();

// Interpolated strings resolve correctly (Car overrides the 'ToString()' method).
await tp.Test($"Rented car should be '{car}'.", async () =>
{
    // Body content in form of given reference type. (Works only for JSON structured bodies).
    var retrievedCar = await tp.Response.GetBodyAsync<Car>();

    Equal(retrievedCar.Brand, car.Brand);
    Equal(retrievedCar.Model, car.Model);
    Equal(retrievedCar.Year, car.Year);
});

ClearVariables();

tp.Logger.LogInformation("End of demo collection testing.");
