#load "./Definitions/GenerateNewCar.csx"

using System.Text.Json;

var car = GenerateCar();
var carJson = JsonSerializer.Serialize(car);

tp.SetVariable("NewCarBody", carJson, "cars");
