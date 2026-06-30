using FluentAssertions;
using TeaPie.Variables;
using Environment = TeaPie.Environments.Environment;

namespace TeaPie.Tests.Environments;

public class EnvironmentShould
{
    [Fact]
    public void SetNameFromConstructor()
    {
        var env = new Environment("dev", new Dictionary<string, object?>());

        env.Name.Should().Be("dev");
    }

    [Fact]
    public void SetVariablesFromConstructor()
    {
        var vars = new Dictionary<string, object?> { { "key", "value" } };
        var env = new Environment("dev", vars);

        env.Variables.Should().ContainKey("key");
    }

    [Fact]
    public void ApplySetsAllVariablesOnTargetCollection()
    {
        var vars = new Dictionary<string, object?>
        {
            { "baseUrl", "http://localhost" },
            { "port", 8080 }
        };
        var env = new Environment("dev", vars);
        var collection = new VariablesCollection();

        env.Apply(collection);

        collection.Get<string>("baseUrl").Should().Be("http://localhost");
        collection.Get<int>("port").Should().Be(8080);
    }
}
