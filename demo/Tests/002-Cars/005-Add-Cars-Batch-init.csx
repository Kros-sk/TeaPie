// Demonstrates the templating loop feature: a request block can be repeated once per
// item in a collection, producing N independent requests instead of one hand-written request.
#load "$teapie/Definitions/GenerateNewCar.csx"

// Generate a batch of fake cars - each one becomes its own POST request in the loop below.
var newCars = GenerateCars(5);

tp.SetVariable("NewCarsBatch", newCars, "cars");
tp.SetVariable("NewCarsBatchCount", newCars.Count, "cars");

tp.SetVariable("NewCarsBatchBrands", newCars.Select(car => car.Brand).ToList(), "cars");
