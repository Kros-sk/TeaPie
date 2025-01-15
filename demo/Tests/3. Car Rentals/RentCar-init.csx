#nuget "Mapster, 7.4.0"
#nuget "Mapster.Core, 1.2.1"

using Mapster;
using Newtonsoft.Json.Linq;

tp.TestCaseVariables.Set("ApiCarRentalSection", "/rental");
tp.CollectionVariables.Set("aaa", "/rental");

TypeAdapterConfig<JObject, Car>.NewConfig()
    .Map(dest => dest.Brand, src => src["Brand"].Value<string>())
    .Map(dest => dest.Model, src => src["Model"].Value<string>())
    .Map(dest => dest.Year, src => src["Year"].Value<long>());

public record Car(string Brand, string Model, long Year)
{
    public override string ToString()
        => $"{Brand} {Model}, {Year}";
}
