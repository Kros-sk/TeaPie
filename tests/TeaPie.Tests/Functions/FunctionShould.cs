using FluentAssertions;
using TeaPie.Functions;

namespace TeaPie.Tests.Functions;

public class FunctionShould
{
    [Fact]
    public void InvokeNoArgFunctionAndReturnResult()
    {
        var func = new Function<int>("$test", () => 42);

        var result = func.InvokeFunction();

        result.Should().Be(42);
    }

    [Fact]
    public void SetNamePropertyFromConstructor()
    {
        var func = new Function<int>("$myFunc", () => 0);

        func.Name.Should().Be("$myFunc");
    }

    [Fact]
    public void InvokeSingleArgFunctionWithConversion()
    {
        var func = new Function<int, int>("$double", x => x * 2);

        var result = func.InvokeFunction(5);

        result.Should().Be(10);
    }

    [Fact]
    public void ThrowWhenSingleArgFunctionCalledWithNoArgs()
    {
        var func = new Function<int, int>("$double", x => x * 2);

        var act = () => func.InvokeFunction(Array.Empty<object>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ThrowWhenSingleArgFunctionCalledWithNullArgs()
    {
        var func = new Function<int, int>("$double", x => x * 2);

        var act = () => func.InvokeFunction(null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void InvokeTwoArgFunctionCorrectly()
    {
        var func = new Function<int, int, int>("$add", (a, b) => a + b);

        var result = func.InvokeFunction(3, 7);

        result.Should().Be(10);
    }

    [Fact]
    public void ThrowWhenTwoArgFunctionCalledWithLessThanTwoArgs()
    {
        var func = new Function<int, int, int>("$add", (a, b) => a + b);

        var act = () => func.InvokeFunction(1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ThrowWhenTwoArgFunctionCalledWithNullArgs()
    {
        var func = new Function<int, int, int>("$add", (a, b) => a + b);

        var act = () => func.InvokeFunction(null);

        act.Should().Throw<ArgumentException>();
    }
}
