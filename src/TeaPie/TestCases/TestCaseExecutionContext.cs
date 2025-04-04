using TeaPie.Scripts;
using TeaPie.StructureExploration;
using TeaPie.Testing;

namespace TeaPie.TestCases;

internal class TestCaseExecutionContext(TestCase testCase) : IExecutionContextExposer
{
    private static int _testCaseIndexer = 1;
    public int Id { get; } = _testCaseIndexer++;

    public TestCase TestCase { get; } = testCase;
    public string? RequestsFileContent;

    private readonly Dictionary<string, ScriptExecutionContext> _preRequestScripts = [];
    private readonly Dictionary<string, ScriptExecutionContext> _postResponseScripts = [];
    public IReadOnlyDictionary<string, ScriptExecutionContext> PreRequestScripts => _preRequestScripts;
    public IReadOnlyDictionary<string, ScriptExecutionContext> PostResponseScripts => _postResponseScripts;

    private readonly Dictionary<string, HttpRequestMessage> _requests = [];
    private readonly Dictionary<string, HttpResponseMessage> _responses = [];
    public IReadOnlyDictionary<string, HttpRequestMessage> Requests => _requests;
    public IReadOnlyDictionary<string, HttpResponseMessage> Responses => _responses;
    public HttpRequestMessage? Request { get; private set; }
    public HttpResponseMessage? Response { get; private set; }

    private readonly Dictionary<string, Test> _tests = [];

    public void RegisterTest(Test test)
        => _tests.Add(test.Name, test);

    public void RegisterRequest(HttpRequestMessage request, string name = "")
    {
        Request = request;
        if (!name.Equals(string.Empty))
        {
            _requests.Add(name, request);
        }
    }

    public void RegisterResponse(HttpResponseMessage response, string name = "")
    {
        Response = response;
        if (!name.Equals(string.Empty))
        {
            _responses.Add(name, response);
        }
    }

    public void RegisterPreRequestScript(string key, ScriptExecutionContext scriptExecutionContext)
        => _preRequestScripts.Add(key, scriptExecutionContext);

    public void RegisterPostResponseScript(string key, ScriptExecutionContext scriptExecutionContext)
        => _postResponseScripts.Add(key, scriptExecutionContext);
}
