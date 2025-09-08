using FluentAssertions;

namespace TeaPie.Tests.Functions;

public class FunctionsShould
{
    [Fact]
    public void OverwriteDefaultFunctionIsNotPosiible()
    {
        var functions = new global::TeaPie.Functions.Functions();
        functions.Register("$guid", () => 1);
        functions.Execute<object>("$guid").Should().NotBe(1);
    }

    [Fact]
    public void OverwriteFunctionsCorrectly()
    {
        var functions = new global::TeaPie.Functions.Functions();
        functions.Register("$FunctionWithOverwrittenValue", () => 1);
        functions.Register("$FunctionWithOverwrittenValue", () => 2);
        functions.Register("$FunctionWithOverwrittenValue", () => 3);

        functions.Execute<int>("$FunctionWithOverwrittenValue").Should().Be(3);
    }
}
