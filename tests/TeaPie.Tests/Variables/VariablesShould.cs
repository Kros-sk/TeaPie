using FluentAssertions;
using TeaPie.Variables;

namespace TeaPie.Tests.Variables;

public class VariablesShould
{
    [Theory]
    [MemberData(nameof(GetAllLevels))]
    public void SetVariableOnEachLevelCorrectly<T>(Func<VariablesCollection> levelGetter, string name, T value)
    {
        levelGetter().Set(name, value);

        levelGetter().Contains(name).Should().BeTrue();
        levelGetter().Get<T>(name).Should().Be(value);
    }

    [Fact]
    private void PrioritizeLevelsCorrectlyWhenGettingVariableSetOnMultipleLevels()
    {
        var variables = new global::TeaPie.Variables.Variables();

        variables.GlobalVariables.Set("VariableOnMultipleLevels", "abc");
        variables.EnvironmentVariables.Set("VariableOnMultipleLevels", "def");
        variables.CollectionVariables.Set("VariableOnMultipleLevels", "ghi");
        variables.TestCaseVariables.Set("VariableOnMultipleLevels", "jkl");

        variables.GetVariable<string>("VariableOnMultipleLevels").Should().Be("jkl");
        variables.GlobalVariables.Get<string>("VariableOnMultipleLevels").Should().Be("abc");
        variables.EnvironmentVariables.Get<string>("VariableOnMultipleLevels").Should().Be("def");
        variables.CollectionVariables.Get<string>("VariableOnMultipleLevels").Should().Be("ghi");
        variables.TestCaseVariables.Get<string>("VariableOnMultipleLevels").Should().Be("jkl");
    }

    [Fact]
    public void HandleVariablesWithTagsCorrectly()
    {
        var variables = new global::TeaPie.Variables.Variables();
        variables.SetVariable("VariableWithoutTag", "noTagVariable");

        variables.SetVariable("VariableWithTestTag1", "testVariable", "test");
        variables.EnvironmentVariables.Set("VariableWithTestTag2", "testVariable", "test");
        variables.TestCaseVariables.Set("VariableWithTestTag3", "testVariable", "test");

        variables.SetVariable("VariableWithOtherTag1", "testVariable", "other");
        variables.SetVariable("VariableWithOtherTag2", "testVariable", "other");

        variables.RemoveVariablesWithTag("test");

        variables.ContainsVariable("VariableWithTestTag1").Should().BeFalse();
        variables.ContainsVariable("VariableWithTestTag2").Should().BeFalse();
        variables.ContainsVariable("VariableWithTestTag3").Should().BeFalse();
        variables.ContainsVariable("VariableWithoutTag").Should().BeTrue();
        variables.ContainsVariable("VariableWithOtherTag1").Should().BeTrue();
        variables.ContainsVariable("VariableWithOtherTag2").Should().BeTrue();
    }

    [Fact]
    private static void OverwriteVariablesCorrectly()
    {
        var variables = new global::TeaPie.Variables.Variables();
        variables.SetVariable("VariableWithOverwrittenValue", 1);
        variables.SetVariable("VariableWithOverwrittenValue", 2);
        variables.SetVariable("VariableWithOverwrittenValue", 3);

        variables.GetVariable<int>("VariableWithOverwrittenValue").Should().Be(3);
    }

    public static IEnumerable<object[]> GetAllLevels()
    {
        var variables = new global::TeaPie.Variables.Variables();
        yield return [() => variables.GlobalVariables, "GlobalVariable", 1];
        yield return [() => variables.EnvironmentVariables, "EnvironmentVariable", -12.34];
        yield return [() => variables.CollectionVariables, "CollectionVariable", 56.78m];
        yield return [() => variables.TestCaseVariables, "TestCaseVariable", "string"];
    }
}
