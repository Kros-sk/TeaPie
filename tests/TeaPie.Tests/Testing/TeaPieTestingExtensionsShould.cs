using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Http;
using TeaPie.Http.Parsing;
using TeaPie.Reporting;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
using TeaPie.Testing;
using static Xunit.Assert;
using Folder = TeaPie.StructureExploration.Folder;
using TestCase = TeaPie.StructureExploration.TestCase;

namespace TeaPie.Tests.Testing;

[Collection(nameof(NonParallelCollection))]
public class TeaPieTestingExtensionsShould
{
    [Fact]
    public void ExecuteTestCorrectly()
    {
        var tester = PrepareTester();
        var teaPie = PrepareTeaPieInstance(tester);
        var executed = false;

        teaPie.Test("", () => executed = true);

        True(executed);
    }

    [Fact]
    public void SkipTestCorrectly()
    {
        var tester = PrepareTester();
        var teaPie = PrepareTeaPieInstance(tester);
        var executed = false;

        teaPie.Test("", () => executed = true, true);

        False(executed);
    }

    [Fact]
    public async Task ExecuteAsyncTestCorrectly()
    {
        var tester = PrepareTester();
        var teaPie = PrepareTeaPieInstance(tester);
        var executed = false;

        await teaPie.Test("", async () =>
        {
            executed = true;
            await Task.CompletedTask;
        });

        True(executed);
    }

    [Fact]
    public async Task SkipAsyncTestCorrectly()
    {
        var tester = PrepareTester();
        var teaPie = PrepareTeaPieInstance(tester);
        var executed = false;

        await teaPie.Test("", async () =>
        {
            executed = true;
            await Task.CompletedTask;
        }, true);

        False(executed);
    }

    [Fact]
    public void RegisterNewTestDirective()
    {
        var testFactory = new TestFactory();
        var teaPie = new TeaPieBuilder().WithService<ITestFactory>(testFactory).Build();

        const string directiveName = "SUCCESSFUL-STATUS";
        const string parameterName = "MyBool";
        var directive = GetDirective(directiveName, parameterName);

        teaPie.RegisterTestDirective(directive.Name, directive.Pattern, directive.TestNameGetter, directive.TestFunction);

        var description = new TestDescription(
            TestDirectives.TestDirectivePrefix + directiveName,
            new Dictionary<string, string>() { { parameterName, "tRuE" } }
        );

        description.SetRequestExecutionContext(new RequestExecutionContext(null!));

        var test = testFactory.Create(description);

        var parser = new TestDirectivesLineParser();

        NotNull(test);
        Contains("Response status code should", test.Name);
        True(parser.CanParse("## TEST-SUCCESSFUL-STATUS: True", new HttpParsingContext(new HttpClient().DefaultRequestHeaders)));
    }

    private static TestDirective GetDirective(string directiveName, string parameterName)
        => new(directiveName,
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

    private static Registrator PrepareTester()
    {
        var accessor = new CurrentTestCaseExecutionContextAccessor()
        {
            Context = new TestCaseExecutionContext(
                new TestCase(
                    InternalFile.Create("path", new Folder(string.Empty, string.Empty, string.Empty, null))))
        };

        return new(accessor);
    }

    private static TeaPie PrepareTeaPieInstance(IRegistrator tester)
        => new TeaPieBuilder().WithService(tester).Build();
}
