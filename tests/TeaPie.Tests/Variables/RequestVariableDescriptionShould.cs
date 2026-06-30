using FluentAssertions;
using TeaPie.Variables;

namespace TeaPie.Tests.Variables;

public class RequestVariableDescriptionShould
{
    [Fact]
    public void ReturnDotSeparatedValuesFromToString()
    {
        var description = new RequestVariableDescription("req1", "response", "body", "$.name");
        description.ToString().Should().Be("req1.response.body.$.name");
    }

    [Fact]
    public void SupportRecordEquality()
    {
        var a = new RequestVariableDescription("req1", "response", "body", "$.name");
        var b = new RequestVariableDescription("req1", "response", "body", "$.name");
        a.Should().Be(b);
    }

    [Fact]
    public void ExposeProperties()
    {
        var description = new RequestVariableDescription("req1", "response", "body", "$.name");
        description.Name.Should().Be("req1");
        description.Type.Should().Be("response");
        description.Content.Should().Be("body");
        description.Query.Should().Be("$.name");
    }
}
