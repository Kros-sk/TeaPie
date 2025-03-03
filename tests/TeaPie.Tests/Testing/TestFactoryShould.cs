using TeaPie.Http;
using TeaPie.Http.Parsing;
using TeaPie.Testing;
using static Xunit.Assert;

namespace TeaPie.Tests.Testing;

public class TestFactoryShould
{
    private readonly TestFactory _factory = new();

    [Fact]
    public void ThrowExceptionForUnsupportedTestType()
    {
        var description = new TestDescription("NOT-SUPORTED-DIRECTIVE", new Dictionary<string, string>());

        var exception = Throws<InvalidOperationException>(() => _factory.Create(description));
        Contains("Unable to create test for unsupported test directive", exception.Message);
    }

    [Fact]
    public void CreateReturnExpectStatusCodesTest()
    {
        var description = new TestDescription(
            TestDirectives.TestExpectStatusCodesDirectiveFullName,
            new Dictionary<string, string>() { { TestDirectives.TestExpectStatusCodesParameterName, "[200, 201]" } }
        );

        description.SetRequestExecutionContext(new RequestExecutionContext(null!));

        var test = _factory.Create(description);

        NotNull(test);
        Contains("Status code should match one of these", test.Name);
    }

    [Fact]
    public void CreateReturnHasBodyTest()
    {
        var description = new TestDescription(
            TestDirectives.TestHasBodyDirectiveFullName,
            new Dictionary<string, string>() { { TestDirectives.TestHasBodyDirectiveParameterName, "true" } }
        );

        description.SetRequestExecutionContext(new RequestExecutionContext(null!));

        var test = _factory.Create(description);

        NotNull(test);
        Contains("Response should have body", test.Name);
    }

    [Fact]
    public void CreateReturnSimplifiedHasBodyTest()
    {
        var description = new TestDescription(
            TestDirectives.TestHasBodyNoParameterInternalDirectiveFullName,
            new Dictionary<string, string>()
        );

        description.SetRequestExecutionContext(new RequestExecutionContext(null!));

        var test = _factory.Create(description);

        NotNull(test);
        Contains("Response should have body", test.Name);
    }

    [Fact]
    public void CreateReturnHasHeaderTest()
    {
        var description = new TestDescription(
            TestDirectives.TestHasHeaderDirectiveFullName,
            new Dictionary<string, string>() { { TestDirectives.TestHasHeaderDirectiveParameterName, "Authorization" } }
        );

        description.SetRequestExecutionContext(new RequestExecutionContext(null!));

        var test = _factory.Create(description);

        NotNull(test);
        Contains("Response should have header with name 'Authorization'", test.Name);
    }

    [Fact]
    public void RegisterNewDirectiveCorrectly()
    {
        const string directiveName = "SUCCESSFUL-STATUS";
        const string parameterName = "MyBool";

        var directive = new TestDirective(
            directiveName,
            TestDirectivePatternBuilder
                .Create(directiveName)
                .AddBooleanParameter(parameterName)
                .Build(),
            (parameters) =>
            {
                var negation = bool.Parse(parameters[parameterName]) ? string.Empty : "NOT ";
                return $"Response status code should {negation}be successful.";
            },
            async (response, parameters) =>
            {
                if (bool.Parse(parameters[parameterName]))
                {
                    True(response.IsSuccessStatusCode);
                }
                else
                {
                    False(response.IsSuccessStatusCode);
                }

                await Task.CompletedTask;
            }
        );

        _factory.RegisterTestDirective(directive);

        var description = new TestDescription(
            directiveName,
            new Dictionary<string, string>() { { parameterName, "tRuE" } }
        );

        description.SetRequestExecutionContext(new RequestExecutionContext(null!));

        var test = _factory.Create(description);

        NotNull(test);
        Contains("Response status code should", test.Name);
    }
}
