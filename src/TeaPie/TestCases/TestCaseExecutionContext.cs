using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.Testing;

namespace TeaPie.TestCases;

internal class TestCaseExecutionContext(TestCase testCase) : IExecutionContextExposer
{
    public TestCase TestCase { get; } = testCase;
    public string? RequestsFileContent;
    public Dictionary<string, ScriptExecutionContext> PreRequestScripts { get; set; } = [];
    public Dictionary<string, ScriptExecutionContext> PostResponseScripts { get; set; } = [];

    public Dictionary<string, HttpRequestMessage> Requests { get; set; } = [];
    public Dictionary<string, HttpResponseMessage> Responses { get; set; } = [];
    public HttpRequestMessage? Request { get; set; }
    public HttpResponseMessage? Response { get; set; }

    public ITestManager TestManager { get; set; } = new TestManager();
}
