using FluentAssertions;
using NSubstitute;
using System.Text;
using TeaPie.Variables;

namespace TeaPie.Tests.Variables;

public class VariablesResolverShould
{
    private const string VariableName = "MyVariable";

    [Fact]
    public void ReturnSameLineIfLineDoesntContainAnyVariableNotation()
    {
        const string line = "Console.Writeline(\"Hello World!\");";
        var resolver = new VariablesResolver(Substitute.For<IVariables>());

        resolver.ResolveVariablesInLine(line).Should().BeEquivalentTo(line);
    }

    [Fact]
    public void ReturnSameLineIfVariableNameViolatesNamingConventions()
    {
        const string invalidVariableName = "My.Variable";
        var line = "Console.Writeline(" + GetVariableNotation(invalidVariableName) + ");";

        var resolver = new VariablesResolver(Substitute.For<IVariables>());

        resolver.ResolveVariablesInLine(line).Should().BeEquivalentTo(line);
    }

    [Fact]
    public void ThrowExceptionWhenAttemptingToResolveNonExistingVariable()
    {
        var line = "Console.Writeline(" + GetVariableNotation(VariableName) + ");";

        var resolver = new VariablesResolver(Substitute.For<IVariables>());

        resolver.Invoking(r => r.ResolveVariablesInLine(line)).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ResolveClassVariableAsItsStringRepresentation()
    {
        DummyPerson variableValue = new(0, "Joseph Carrot");
        var line = "Console.Writeline(" + GetVariableNotation(VariableName) + ");";
        var resolvedLine = "Console.Writeline(" + variableValue + ");";

        var variables = new global::TeaPie.Variables.Variables();
        var resolver = new VariablesResolver(variables);

        variables.SetVariable(VariableName, variableValue);
        resolver.ResolveVariablesInLine(line).Should().BeEquivalentTo(resolvedLine);
    }

    [Fact]
    public void ResolveVariableWithNullValueAsNullString()
    {
        DummyPerson? variableValue = null;
        var line = "Console.Writeline(" + GetVariableNotation(VariableName) + ");";
        const string resolvedLine = "Console.Writeline(null);";

        var variables = new global::TeaPie.Variables.Variables();
        var resolver = new VariablesResolver(variables);

        variables.SetVariable(VariableName, variableValue);
        resolver.ResolveVariablesInLine(line).Should().BeEquivalentTo(resolvedLine);
    }

    [Fact]
    public void ResolveSingleVariableInSingleLineCorrectly()
    {
        const string variableValue = "Hello World!";
        var line = "Console.Writeline(" + GetVariableNotation(VariableName) + ");";
        const string resolvedLine = "Console.Writeline(" + variableValue + ");";

        var variables = new global::TeaPie.Variables.Variables();
        var resolver = new VariablesResolver(variables);

        variables.SetVariable(VariableName, variableValue);
        resolver.ResolveVariablesInLine(line).Should().BeEquivalentTo(resolvedLine);
    }

    [Fact]
    public void ResolveMultipleVariablesInSingleLineCorrectly()
    {
        const int count = 10;
        var lineBuilder = new StringBuilder();
        var resolvedLineBuilder = new StringBuilder();
        var variablesNames = new string[count];
        var variablesValues = new string[count];

        var variables = new global::TeaPie.Variables.Variables();
        var resolver = new VariablesResolver(variables);

        for (var i = 0; i < count; i++)
        {
            variablesNames[i] = VariableName + i.ToString();
            lineBuilder.Append(GetVariableNotation(variablesNames[i]));
        }

        for (var i = 0; i < count; i++)
        {
            variablesValues[i] = Path.GetRandomFileName();
            resolvedLineBuilder.Append(variablesValues[i]);
            variables.SetVariable(variablesNames[i], variablesValues[i]);
        }

        resolver.ResolveVariablesInLine(lineBuilder.ToString()).Should().BeEquivalentTo(resolvedLineBuilder.ToString());
    }

    private static string GetVariableNotation(string variableName) => "{{" + variableName + "}}";

    private class DummyPerson(int id, string name)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;

        public override string ToString() => $"My name is {Name} and my identification number is {Id}.";
    }
}
