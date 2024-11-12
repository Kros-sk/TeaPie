#nuget "System.IO.Abstractions, 13.2.6"
#nuget "System.Net.Http, 4.3.4"
#nuget "FluentAssertions, 6.12.1"
#nuget "Newtonsoft.Json, 13.0.3"

using FluentAssertions;
using Newtonsoft.Json;
using System.IO.Abstractions;

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }

    public override string ToString() => $"{Name}, {Age}";
}

var personJson = "{ \"name\": \"Abraham Lincoln\", \"age\": 24 }";
Console.WriteLine("Person in JSON format:" + personJson);

var person = JsonConvert.DeserializeObject<Person>(personJson);
Console.WriteLine("JSON deserialized to Person class: " + person.ToString());

person.Age.Should().Be(24);

IFileSystem fileSystem = new FileSystem();

string filePath = $"{person.Name.Replace(" ", string.Empty)}.txt";

if (!fileSystem.File.Exists(filePath))
{
    fileSystem.File.WriteAllText(filePath, $"Hello, {person.ToString()}!");
}

string content = fileSystem.File.ReadAllText(filePath);
Console.WriteLine("File content: " + content);

fileSystem.File.Delete(filePath);
