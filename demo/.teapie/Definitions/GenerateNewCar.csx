#load "CarFaker.csx"

using AutoBogus;

public Car GenerateCar()
{
    var faker = new CarFaker();
    return faker.Generate();
}

public List<Car> GenerateCars(int count)
{
    var faker = new CarFaker();
    return faker.Generate(count);
}

