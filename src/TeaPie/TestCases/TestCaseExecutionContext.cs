using TeaPie.Http;
using TeaPie.Scripts;
using TeaPie.StructureExploration;

namespace TeaPie.TestCases;

internal class TestCaseExecutionContext(TestCase testCase)
{
    public TestCase TestCase { get; set; } = testCase;
    public string? RequestsFileContent;
    public Dictionary<string, RequestExecutionContext> RequestExecutionContexts { get; set; } = [];
    public Dictionary<string, ScriptExecutionContext> PreRequestScripts { get; set; } = [];
    public Dictionary<string, ScriptExecutionContext> PostResponseScripts { get; set; } = [];
    public Dictionary<string, HttpRequestMessage> Requests { get; set; } = [];
    public Dictionary<string, HttpResponseMessage> Responses { get; set; } = [];
    public HttpRequestMessage? Request { get; set; }
    public HttpResponseMessage? Response { get; set; }
}
