using FluentAssertions;
using TeaPie.Exceptions;
using TeaPie.Variables;

namespace TeaPie.Tests.Variables;

public class VariablesCollectionShould
{
    [Theory]
    [InlineData(null, "null name")]
    [InlineData("", "empty name")]
    [InlineData(".", "name with forbidden character '.'")]
    [InlineData("$", "name with forbidden character '$'")]
    [InlineData("_", "name with forbidden character '_'")]
    [InlineData("<", "name with forbidden character '<'")]
    [InlineData(">", "name with forbidden character '>'")]
    [InlineData(".$_<script>", "name with forbidden characters")]
    public void ThrowProperExceptionWhenSettingVariableWithInvalidName(string? name, string reason)
    {
        VariablesCollection collection = [];

        collection.Invoking(coll => coll.Set(name!, 20.32)).Should()
            .Throw<VariableNameViolationException>()
            .WithMessage("*", because: $"Variable can not have {reason}.");
    }

    [Fact]
    public void SetSingleVariableWithNullValueWithoutAnyProblem()
    {
        VariablesCollection collection = [];
        const string name = "NullVariable";
        collection.Set<object>(name, null);

        var enumerator = collection.GetEnumerator();
        enumerator.MoveNext();

        enumerator.Current.GetValue<object>().Should().BeNull();
        collection.Contains(name).Should().BeTrue();
    }

    [Fact]
    public void SetSingleVariableWithoutAnyProblem()
    {
        VariablesCollection collection = [];
        const string name = "MyAge";
        const int value = 24;

        collection.Set(name, value);
        var result = collection.Get<int>(name);

        result.Should().Be(value);
        collection.Contains(name).Should().BeTrue();
    }

    [Fact]
    public void SetAndThenGetTheSameVariable()
    {
        VariablesCollection collection = [];
        const string name = "MyAge";
        const int value = 42;

        collection.Set(name, value);
        var result = collection.Get<int>(name);

        result.Should().Be(value);
    }

    [Fact]
    public void OverwriteVariableWhenSettingVariableWithSameNameTwice()
    {
        VariablesCollection collection = [];
        const string name = "MyAge";
        const int value1 = 24;
        const int value2 = 42;

        collection.Set(name, value1);
        collection.Set(name, value2);

        var result = collection.Get<int>(name);

        result.Should().Be(value2);
    }

    [Fact]
    public void SetMultipleVariablesWithoutAnyProblem()
    {
        VariablesCollection collection = [];
        const int numberOfVariables = 10;

        var names = new string[numberOfVariables];
        var values = new decimal[numberOfVariables];

        for (var i = 0; i < numberOfVariables; i++)
        {
            names[i] = Guid.NewGuid().ToString();
            values[i] = i * 23.5m;

            collection.Set(names[i], values[i]);
        }

        collection.Count.Should().Be(numberOfVariables);

        for (var i = 0; i < numberOfVariables; i++)
        {
            var result = collection.Get<decimal>(names[i]);
            result.Should().Be(values[i]);
            collection.Contains(names[i]).Should().BeTrue();
        }
    }

    [Fact]
    public void ReturnDefaultValueWhenGettingVariableWithNotMatchingDataType()
    {
        VariablesCollection collection = [];
        const string name = "MyVariable";

        collection.Set(name, 2000);
        var result = collection.Get<Dummy>(name);

        result.Should().BeNull();
    }

    [Theory]
    [InlineData("test")]
    [InlineData("user", "auth", "test")]
    public void SetVariableWithTagsWithoutAnyProblem(params string[] tags)
    {
        VariablesCollection collection = [];
        const string name = "VariableWithTags";
        const string value = "I am variable with tags!";

        collection.Set(name, value, tags);

        var enumerator = collection.GetEnumerator();
        enumerator.MoveNext();

        var variable = enumerator.Current;

        variable.Value.Should().Be(value);
        foreach (var tag in tags)
        {
            variable.HasTag(tag).Should().BeTrue();
        }
    }

    [Fact]
    public void IndicatedRemovalOfNonExistingByReturningFalse()
    {
        VariablesCollection collection = [];
        const string name = "MyVariable";
        const string nonExistingVariableName = "NonExisting";

        collection.Set(name, true);
        var resultOfRemoval = collection.Remove(nonExistingVariableName);

        resultOfRemoval.Should().BeFalse();
    }

    [Fact]
    public void RemoveVariableWithoutAnyProblem()
    {
        VariablesCollection collection = [];
        const string name = "MyVariable";

        collection.Set(name, true);
        var resultOfRemoval = collection.Remove(name);

        resultOfRemoval.Should().BeTrue();
    }

    [Theory]
    [InlineData("test")]
    [InlineData("auth", "test")]
    [InlineData("user", "auth", "test")]
    [InlineData("user")]
    public void RemoveAllVariablesWithGivenTag(params string[] tagsToDelete)
    {
        VariablesCollection collection = [];

        AddTaggedVariables(collection);

        IEnumerable<Variable>? variablesToRemain = null;
        foreach (var tag in tagsToDelete)
        {
            collection.RemoveVariablesWithTag(tag);
            if (variablesToRemain is null)
            {
                variablesToRemain = collection.Where(var => !var.HasTag(tag));
            }
            else
            {
                variablesToRemain = variablesToRemain.Intersect(collection.Where(var => !var.HasTag(tag)));
            }
        }

        variablesToRemain.Should().NotBeNull();
        collection.Count.Should().Be(variablesToRemain!.Count());

        int result;
        foreach (var variable in variablesToRemain!)
        {
            result = collection.Get<int>(variable.Name);
            result.Should().Be(variable.GetValue<int>());
        }
    }

    private class Dummy() { }

    private static void AddTaggedVariables(VariablesCollection collection)
    {
        collection.Set("name1", 1);
        collection.Set("name2", 2);
        collection.Set("name3", 3);
        collection.Set("name4", 4, "test");
        collection.Set("name5", 5, "test");
        collection.Set("name6", 6, "test");
        collection.Set("name7", 7, "test", "auth");
        collection.Set("name8", 8, "test", "auth");
        collection.Set("name9", 9, "test", "auth");
        collection.Set("name10", 10, "test", "user");
        collection.Set("name11", 11, "test", "user");
        collection.Set("name12", 12, "test", "user");
        collection.Set("name13", 13, "test", "auth", "user");
        collection.Set("name14", 14, "test", "auth", "user");
        collection.Set("name15", 15, "test", "auth", "user");
        collection.Set("name16", 16, "user");
        collection.Set("name17", 17, "user");
        collection.Set("name18", 18, "user");
    }
}
