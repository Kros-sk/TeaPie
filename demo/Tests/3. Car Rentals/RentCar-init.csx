// Use 'nuget' directives to download NuGet packages with their dependencies.
// Downloaded libraries are globally available across scripts but require a 'using' directive to access their functionality.
#nuget "Mapster, 7.4.0"
#nuget "Mapster.Core, 1.2.1"

using Mapster;
using Newtonsoft.Json.Linq;

// Store variables in 'TestCaseVariables' for the test case life-cycle or 'CollectionVariables' for broader scope.
tp.TestCaseVariables.Set("Brand", "Toyota", "cars");
tp.TestCaseVariables.Set("Model", "RAV4", "cars");
tp.TestCaseVariables.Set("Year", 2022, "cars");
tp.CollectionVariables.Set("ApiCarRentalSection", "/rental");

// Configure Mapster for mapping JSON objects to a 'Car' object.
TypeAdapterConfig<JObject, Car>.NewConfig()
    .Map(dest => dest.Brand, src => src["Brand"].Value<string>())
    .Map(dest => dest.Model, src => src["Model"].Value<string>())
    .Map(dest => dest.Year, src => src["Year"].Value<long>());

// Classes, records, or structs can be defined in scripts.
// To reuse them in other scripts, reference the script containing the class definition.
public record Car(string Brand, string Model, long Year)
{
    public override string ToString()
        => $"{Brand} {Model}, {Year}";
}
