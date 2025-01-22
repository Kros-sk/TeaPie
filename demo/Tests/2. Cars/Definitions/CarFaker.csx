// Use 'nuget' directives to download NuGet packages with their dependencies.
#nuget "AutoBogus, 2.13.1"
#nuget "Bogus, 31.0.3"

// Reference script with class definition
#load "Car.csx"

using AutoBogus;

public class CarFaker : AutoFaker<Car>
{
    public CarFaker()
    {
        RuleFor(c => c.Id, f => f.Random.Long(7, 100));
        RuleFor(c => c.Brand, f => f.PickRandom(new[] { "Tesla", "BMW", "Audi", "Ford", "Toyota" }));
        RuleFor(c => c.Model, (f, c) => GenerateModelByBrand(c.Brand, f));
        RuleFor(c => c.EngineType, f => f.PickRandom(new[] { "Petrol", "Diesel", "Electric", "Hybrid" }));
        RuleFor(c => c.TransmissionType, f => f.PickRandom(new[] { "Manual", "Automatic" }));
        RuleFor(c => c.PeopleCapacity, f => f.Random.Int(2, 7));
        RuleFor(c => c.Color, f => f.Commerce.Color());
        RuleFor(c => c.Year, f => f.Random.Int(1990, DateTime.Now.Year));
        RuleFor(c => c.DrivenKilometres, f => f.Random.Double(0, 300000));
        RuleFor(c => c.Description, f => f.Lorem.Sentence());
    }

    private static string GenerateModelByBrand(string brand, Bogus.Faker faker)
    {
        return brand switch
        {
            "Tesla" => faker.PickRandom(new[] { "Model S", "Model 3", "Model X", "Model Y" }),
            "BMW" => faker.PickRandom(new[] { "X5", "3 Series", "5 Series", "M3" }),
            "Audi" => faker.PickRandom(new[] { "A4", "A6", "Q7", "R8" }),
            "Ford" => faker.PickRandom(new[] { "F-150", "Mustang", "Explorer", "Focus" }),
            "Toyota" => faker.PickRandom(new[] { "Corolla", "Camry", "RAV4", "Prius" }),
            _ => "Unknown Model"
        };
    }
}
