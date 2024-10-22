#nuget "Newtonsoft.Json, 13.0.3"
#nuget "System.IO.Abstractions, 13.2.6"
#nuget "System.Net.Http, 4.3.4"

using Newtonsoft.Json;
using System.IO.Abstractions;
using System.Net.Http;

tp.Logger.LogInformation("I am script with multiple Nuget Packages and I am about to run (marathon)...");

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }

    public string ToString() => $"{Name}, {Age}";
}

var personJson = "{ \"name\": \"Abraham Lincoln\", \"age\": 24 }";
Console.WriteLine("Person in JSON format:" + personJson);

var person = JsonConvert.DeserializeObject<Person>(personJson);
Console.WriteLine("JSON deserialized to Person class: " + person.ToString());
