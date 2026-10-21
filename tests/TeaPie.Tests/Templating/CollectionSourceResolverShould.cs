using FluentAssertions;
using TeaPie.Templating;

namespace TeaPie.Tests.Templating;

public class CollectionSourceResolverShould
{
    public static TheoryData<string, object?[]> InlineLiterals => new()
    {
        { "(\"new\", \"used\")", ["new", "used"] },
        { "(\"a,b\", \"c\")", ["a,b", "c"] },
        { "(\"Acme\", 42, true, 1.5)", ["Acme", 42, true, 1.5] },
        { "(123456789012345678)", [123456789012345678L] },
        { "()", [] },
        { "(   )", [] }
    };

    [Theory]
    [MemberData(nameof(InlineLiterals))]
    public void ResolveAnInlineLiteralListIntoItsTypedItems(string expression, object?[] expected)
    {
        var source = Resolve(expression);

        source.ItemCount.Should().Be(expected.Length);
        source.Collection.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ResolveANamedCollectionVariable()
    {
        var source = Resolve("FreePartners");

        source.ItemCount.Should().Be(3);
        source.Collection.Should().NotBeNull();
    }

    [Theory]
    [InlineData("(1..5)", 5)]
    [InlineData("(0..2147483647)", int.MaxValue)]
    public void ResolveANumericRangeToItsItemCountWithoutACollection(string expression, int expectedCount)
    {
        var source = Resolve(expression);

        source.ItemCount.Should().Be(expectedCount);
        source.Collection.Should().BeNull();
    }

    [Theory]
    [InlineData("DoesNotExist", "*DoesNotExist*was not found*")]
    [InlineData("NotACollection", "*must be a collection*")]
    [InlineData("AString", "*'AString'*must be a collection*")]
    [InlineData("AnEmptyString", "*'AnEmptyString'*must be a collection*")]
    [InlineData("SecretPartners", "*SecretPartners*tagged as 'secret'*")]
    [InlineData("(1, oops, 3)", "*oops*")]
    [InlineData("(\"a\", \"b\"", "*unbalanced*")]
    [InlineData("(1,,3)", "*empty item*")]
    [InlineData("(1, , 3)", "*empty item*")]
    [InlineData("(1..99999999999)", "*99999999999*")]
    [InlineData("(99999999999..99999999999)", "*99999999999*")]
    public void ThrowAClearErrorForAnUnresolvableSource(string expression, string expectedMessage)
    {
        var act = () => Resolve(expression);

        act.Should().Throw<InvalidOperationException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void RejectASecretVariableThatShadowsAPublicOneWithoutRevealingItsValue()
    {
        var variables = new global::TeaPie.Variables.Variables();
        variables.GlobalVariables.Set("Ids", new[] { "PUBLIC" });
        variables.TestCaseVariables.Set("Ids", new[] { "S3CR3T" }, Constants.SecretVariableTag);

        var act = () => Resolve(variables, "Ids");

        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("'Ids'").And.NotContain("S3CR3T");
    }

    private static LoopSource Resolve(string expression)
    {
        var variables = new global::TeaPie.Variables.Variables();
        variables.SetVariable("FreePartners", new List<string> { "01245", "012426", "012427" });
        variables.SetVariable("NotACollection", 42);
        variables.SetVariable("AString", "abc123");
        variables.SetVariable("AnEmptyString", string.Empty);
        variables.SetVariable("SecretPartners", new List<string> { "S3CR3T" }, Constants.SecretVariableTag);
        return Resolve(variables, expression);
    }

    private static LoopSource Resolve(global::TeaPie.Variables.Variables variables, string expression)
        => new CollectionSourceResolver(variables).Resolve(expression, new VariablesFluidModelBuilder().Build(variables));
}
