// Referencing multiple scripts is also allowed.
#load "../ClearVariables.csx"
#load "../2. Cars/Definitions/Car.csx"

// Sometimes, during tests writing it can be useful to skip some tests to fasten the process
// and prevent unecessary code commenting/deletion.
tp.Test("Car should be rented successfully.", () =>
{
    // Access a named response.
    Equal(201, tp.Responses["RentCarRequest"].StatusCode());
    // Access the response from the most recently executed request.
    Equal(200, tp.Response.StatusCode());
}, true); // To skip test, just add optional parameter to 'true'.

// If you have variable in JSON string, it can be easily converted to reference type, by using 'To<TResult>()' method.
var car = tp.GetVariable<string>("NewCar").To<Car>();

// Interpolated strings resolve correctly ('Car' overrides the 'ToString()' method).
await tp.Test($"Rented car should be '{car}'.", async () =>
{
    // Body content in form of given reference type. (Works only for JSON structured bodies).
    var retrievedCar = await tp.Response.GetBodyAsync<Car>();

    Equal(car.Brand, retrievedCar.Brand);
    Equal(car.Model, retrievedCar.Model);
    Equal(car.Year, retrievedCar.Year);
});

ClearVariables();

tp.Logger.LogInformation("End of demo collection testing.");
