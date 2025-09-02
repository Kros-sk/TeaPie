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
    public void ThrowProperExceptionWhenRegisterFunctionWithInvalidName(string? name, string reason)
    {
        FunctionsCollection collection = [];

        collection.Invoking(coll => coll.Register(name!, () => "")).Should()
            .Throw<FunctionNameViolationException>()
            .WithMessage("*", because: $"Function can not have {reason}.");
    }

    [Fact]
    public void RegisterSingleFucntionWithoutAnyProblem()
    {
        FunctionsCollection collection = [];
        const string name = "$MyAge";
        const int value = 24;

        collection.Register(name, () => value);
        var result = collection.Execute<int>(name);

        result.Should().Be(value);
        collection.Contains(name, 0).Should().BeTrue();
    }

    [Fact]
    public void RegisterAndThenExecuteTheSameFunction()
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
    public void RegisterSameFunctionWithDifferentArgumentCount()
    {
        FunctionsCollection collection = [];
        const string name = "$MyAge";
        const int value1 = 24;
        const int value2 = 42;
        const int value3 = 66;

        collection.Register(name, () => value1);
        collection.Register(name, (int val) => val);
        collection.Register(name, (int val, int val2) => val + val2);

        var result = collection.Execute<int>(name);

        collection.Execute<int>(name).Should().Be(value1);
        collection.Execute<int>(name, value2).Should().Be(value2);
        collection.Execute<int>(name, value1, value2).Should().Be(value3);
    }

    [Fact]
    public void RegisterMultipleFunctionsWithoutAnyProblem()
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
            collection.Contains(names[i], 1).Should().BeTrue();
        }
    }
}
