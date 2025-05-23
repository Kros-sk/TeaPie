﻿// Use 'load' directive to reference another script.
// Path can be absolute or relative (relative path starts from the parent directory).
// IMPORTANT: Referenced scripts run automatically. Encapsulate logic in methods to avoid unintended execution.
#load "$teapie/Definitions/GenerateNewCar.csx"
// Placeholder '$teapie' will be replaced with the actual path pointing to .teapie folder.

// Usage of method defined in referenced script.
var car = GenerateCar();

// All objects can be serialized to string with JSON structure, just by calling 'ToJsonString()' extension method.
tp.SetVariable("NewCar", car.ToJsonString(), "cars");
