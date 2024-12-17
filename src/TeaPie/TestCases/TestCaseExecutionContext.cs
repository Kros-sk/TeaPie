using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.Testing;

namespace TeaPie.TestCases;

internal class TestCaseExecutionContext(TestCase testCase) : IExecutionContextExposer, ITestRegistrator
{
    public TestCase TestCase { get; } = testCase;
    public string? RequestsFileContent;
    public Dictionary<string, ScriptExecutionContext> PreRequestScripts { get; set; } = [];
    public Dictionary<string, ScriptExecutionContext> PostResponseScripts { get; set; } = [];

    public Dictionary<string, HttpRequestMessage> Requests { get; set; } = [];
    public Dictionary<string, HttpResponseMessage> Responses { get; set; } = [];
    public HttpRequestMessage? Request { get; set; }
    public HttpResponseMessage? Response { get; set; }

    private readonly Dictionary<string, Test> _tests = [];

    public void RegisterTest(Test test)
        => _tests.Add(test.Name, test);
}
