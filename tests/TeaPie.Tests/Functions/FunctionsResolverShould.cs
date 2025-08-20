using FluentAssertions;
using NSubstitute;
using System.Text;
using TeaPie.Functions;
using TeaPie.Http;

namespace TeaPie.Tests.Functions;

public class FunctionsResolverShould
{
    private const string FunctionName = "$MyFunction";

    [Fact]
    public void ReturnSameLineIfLineDoesntContainAnyFunctionNotation()
    {
        const string line = "Console.Writeline(\"Hello World!\");";
        var resolver = new FunctionsResolver(Substitute.For<IFunctions>());
        var context = GetRequestExecutionContextMock();

        resolver.ResolveFunctionsInLine(line, context).Should().BeEquivalentTo(line);
    }

    [Fact]
    public void ReturnSameLineIfFunctionNameViolatesNamingConventions()
    {
        const string invalidFunctionName = "My<Function>";
        var line = "Console.Writeline(" + GetFunctionNotation(invalidFunctionName) + ");";
        var context = GetRequestExecutionContextMock();

        var resolver = new FunctionsResolver(Substitute.For<IFunctions>());

        resolver.ResolveFunctionsInLine(line, context).Should().BeEquivalentTo(line);
    }

    [Fact]
    public void ThrowProperExceptionWhenAttemptingToResolveNonExistingFunction()
    {
        var line = "Console.Writeline(" + GetFunctionNotation(FunctionName) + ");";
        var context = GetRequestExecutionContextMock();

        var resolver = new FunctionsResolver(Substitute.For<IFunctions>());

        resolver.Invoking(r => r.ResolveFunctionsInLine(line, context)).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ResolveSingleFunctionInSingleLineCorrectly()
    {
        const string value = "Hello World!";
        var line = "Console.Writeline(" + GetFunctionNotation(FunctionName) + ");";
        const string resolvedLine = "Console.Writeline(" + value + ");";

        var functions = new global::TeaPie.Functions.Functions();
        var resolver = new FunctionsResolver(functions);
        var context = GetRequestExecutionContextMock();

        functions.Register(FunctionName, () => value);
        resolver.ResolveFunctionsInLine(line, context).Should().BeEquivalentTo(resolvedLine);
    }

    [Fact]
    public void ResolveClassFunctionAsItsStringRepresentation()
    {
        DummyPerson value = new(0, "Joseph Carrot");
        var line = "Console.Writeline(" + GetFunctionNotation(FunctionName) + ");";
        var resolvedLine = "Console.Writeline(" + value + ");";

        var functions = new global::TeaPie.Functions.Functions();
        var resolver = new FunctionsResolver(functions);
        var context = GetRequestExecutionContextMock();

        functions.Register(FunctionName, () => value);
        resolver.ResolveFunctionsInLine(line, context).Should().BeEquivalentTo(resolvedLine);
    }

    [Fact]
    public void ResolveFunctionWithNullValueAsNullString()
    {
        DummyPerson? value = null;
        var line = "Console.Writeline(" + GetFunctionNotation(FunctionName) + ");";
        const string resolvedLine = "Console.Writeline(null);";

        var functions = new global::TeaPie.Functions.Functions();
        var resolver = new FunctionsResolver(functions);
        var context = GetRequestExecutionContextMock();

        functions.Register(FunctionName, () => value);
        resolver.ResolveFunctionsInLine(line, context).Should().BeEquivalentTo(resolvedLine);
    }

    [Fact]
    public void ResolveMultipleFunctionsInSingleLineCorrectly()
    {
        const int count = 10;
        var lineBuilder = new StringBuilder();
        var resolvedLineBuilder = new StringBuilder();
        var functionsNames = new string[count];
        var functionsValues = new string[count];

        var functions = new global::TeaPie.Functions.Functions();
        var resolver = new FunctionsResolver(functions);
        var context = GetRequestExecutionContextMock();

        for (var i = 0; i < count; i++)
        {
            functionsNames[i] = FunctionName + i.ToString();
            functionsValues[i] = $"Test{i}";
            resolvedLineBuilder.Append(functionsValues[i]);
            lineBuilder.Append($"{{{{{functionsNames[i]} {i}}}}}");
            functions.Register(functionsNames[i], (int i) => $"Test{i}");
        }

        resolver.ResolveFunctionsInLine(lineBuilder.ToString(), context).Should().BeEquivalentTo(resolvedLineBuilder.ToString());
    }

    private static RequestExecutionContext GetRequestExecutionContextMock() => new(null!, null);

    private static string GetFunctionNotation(string functionName) => "{{" + functionName + "}}";

    private class DummyPerson(int id, string name)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;

        public override string ToString() => $"My name is {Name} and my identification number is {Id}.";
    }
}
