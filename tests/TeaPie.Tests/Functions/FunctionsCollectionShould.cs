using FluentAssertions;
using TeaPie.Functions;

namespace TeaPie.Tests.Functions;

public class FunctionsCollectionShould
{
    [Theory]
    [InlineData(null, "null name")]
    [InlineData("", "empty name")]
    [InlineData("<", "name with forbidden character '<'")]
    [InlineData(">", "name with forbidden character '>'")]
    [InlineData("<script>", "name with forbidden characters")]
    public void ThrowProperExceptionWhenSettingFunctionWithInvalidName(string? name, string reason)
    {
        FunctionsCollection collection = [];

        collection.Invoking(coll => coll.Register(name!, () => "")).Should()
            .Throw<FunctionNameViolationException>()
            .WithMessage("*", because: $"Function can not have {reason}.");
    }

    [Fact]
    public void SetSingleFucntionWithoutAnyProblem()
    {
        FunctionsCollection collection = [];
        const string name = "$MyAge";
        const int value = 24;

        collection.Register(name, () => value);
        var result = collection.Execute<int>(name);

        result.Should().Be(value);
        collection.Contains(name).Should().BeTrue();
    }

    [Fact]
    public void SetAndThenExecuteTheSameFunction()
    {
        FunctionsCollection collection = [];
        const string name = "$MyAge";
        const int value = 42;

        collection.Register(name, () => value);
        var result = collection.Execute<int>(name);

        result.Should().Be(value);
    }

    [Fact]
    public void OverwriteFunctionWhenSettingFunctionWithSameNameTwice()
    {
        FunctionsCollection collection = [];
        const string name = "$MyAge";
        const int value1 = 24;
        const int value2 = 42;

        collection.Register(name, () => value1);
        collection.Register(name, () => value2);

        var result = collection.Execute<int>(name);

        result.Should().Be(value2);
    }

    [Fact]
    public void SetMultipleFunctionsWithoutAnyProblem()
    {
        FunctionsCollection collection = [];
        const int numberOfFunctions = 10;

        var names = new string[numberOfFunctions];
        var values = new decimal[numberOfFunctions];

        for (var i = 0; i < numberOfFunctions; i++)
        {
            names[i] = $"${Guid.NewGuid()}";
            values[i] = i * 23.5m;

            collection.Register(names[i], (int i) => i * 23.5m);
        }

        collection.Count.Should().Be(numberOfFunctions);

        for (var i = 0; i < numberOfFunctions; i++)
        {
            var result = collection.Execute<decimal>(names[i], i);
            result.Should().Be(values[i]);
            collection.Contains(names[i]).Should().BeTrue();
        }
    }
}
