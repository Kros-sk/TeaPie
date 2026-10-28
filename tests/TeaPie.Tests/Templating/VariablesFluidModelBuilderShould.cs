using FluentAssertions;
using NSubstitute;
using TeaPie.Templating;

namespace TeaPie.Tests.Templating;

public class VariablesFluidModelBuilderShould
{
    [Fact]
    public void MergeAllScopesPreferringTheHigherPriorityOneAndKeepingValuesAsTheyAre()
    {
        var variables = new global::TeaPie.Variables.Variables();
        var partners = new List<string> { "Acme", "Globex" };
        variables.GlobalVariables.Set("Shared", "global");
        variables.EnvironmentVariables.Set("Shared", "environment");
        variables.CollectionVariables.Set("Shared", "collection");
        variables.TestCaseVariables.Set("Shared", "test-case");
        variables.GlobalVariables.Set("OnlyGlobal", 5.5);
        variables.CollectionVariables.Set("Temp.Partners", partners);
        variables.CollectionVariables.Set<object?>("MaybeNull", null);

        var result = new VariablesFluidModelBuilder().Build(variables);

        result.Should().HaveCount(4);
        result["Shared"].Should().Be("test-case");
        result["OnlyGlobal"].Should().Be(5.5);
        result["Temp.Partners"].Should().BeSameAs(partners);
        result.Should().ContainKey("MaybeNull").WhoseValue.Should().BeNull();
    }

    [Fact]
    public void ExcludeVariablesTaggedAsSecret()
    {
        var variables = new global::TeaPie.Variables.Variables();
        variables.CollectionVariables.Set("AccessToken", "super-secret-token", Constants.SecretVariableTag);
        variables.CollectionVariables.Set("PartnerCount", 3);

        var result = new VariablesFluidModelBuilder().Build(variables);

        result.Should().NotContainKey("AccessToken").And.ContainKey("PartnerCount");
    }

    [Fact]
    public void NotExposeALowerPriorityValueShadowedByASecret()
    {
        var variables = new global::TeaPie.Variables.Variables();
        variables.GlobalVariables.Set("Shadow", "PUBLIC");
        variables.TestCaseVariables.Set("Shadow", "HIDDEN", Constants.SecretVariableTag);

        var result = new VariablesFluidModelBuilder().Build(variables);

        result.Should().NotContainKey("Shadow");
    }

    [Fact]
    public void ReturnAnEmptyModelForAnUnconfiguredIVariables()
        => new VariablesFluidModelBuilder().Build(Substitute.For<global::TeaPie.Variables.IVariables>())
            .Should().BeEmpty();
}
