using FluentAssertions;

namespace TeaPie.Tests.Functions;

public class FunctionsShould
{
    [Fact]
    public void OverwriteDefaultFunctionIsNotPosiible()
    {
        var functions = new global::TeaPie.Functions.Functions();
        functions.Register("$guid", () => 1);
        functions.Execute<int>("$VariableWithOverwrittenValue").Should().NotBe(1);
    }

    [Fact]
    public void OverwriteVariablesCorrectly()
    {
        var functions = new global::TeaPie.Functions.Functions();
        functions.Register("$VariableWithOverwrittenValue", () => 1);
        functions.Register("$VariableWithOverwrittenValue", () => 2);
        functions.Register("$VariableWithOverwrittenValue", () => 3);

        functions.Execute<int>("$VariableWithOverwrittenValue").Should().Be(3);
    }
}
