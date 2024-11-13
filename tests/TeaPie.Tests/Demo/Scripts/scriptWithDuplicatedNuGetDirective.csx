#nuget "Newtonsoft.Json, 13.0.3"
#nuget "Newtonsoft.Json, 13.0.3"

using Newtonsoft.Json;

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
