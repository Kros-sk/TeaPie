#load "./Definitions/GenerateNewCar.csx"

// Usage of method defined in referenced script.
var car = GenerateCar();

// All objects can be serialized to string with JSON structure, just by calling 'ToJsonString()' extension method.
tp.SetVariable("NewCar", car.ToJsonString(), "cars");
