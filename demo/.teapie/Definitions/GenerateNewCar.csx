#load "CarFaker.csx"

using AutoBogus;

public Car GenerateCar()
{
    var faker = new CarFaker();
    return faker.Generate();
}

