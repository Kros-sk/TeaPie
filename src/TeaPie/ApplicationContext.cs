using Microsoft.Extensions.Logging;
using TeaPie.StructureExploration;
using TeaPie.TestCases;

namespace TeaPie;

internal class ApplicationContext(
    string path,
    IServiceProvider serviceProvider,
    ICurrentTestCaseExecutionContextAccessor currentTestCaseExecutionContextAccessor,
    ILogger logger,
    string tempFolder = "")
{
    public string Path { get; } = path;
    public string TempFolderPath { get; set; } = tempFolder;

    public IReadOnlyDictionary<string, TestCase> TestCases { get; set; } = new Dictionary<string, TestCase>();

    private readonly Dictionary<string, Script> _userDefinedScripts = [];
    public IReadOnlyDictionary<string, Script> UserDefinedScripts => _userDefinedScripts;
    public void RegisterUserDefinedScript(string key, Script script) => _userDefinedScripts.Add(key, script);

    public ILogger Logger { get; set; } = logger;

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    private readonly ICurrentTestCaseExecutionContextAccessor _currentTestCaseExecutionContextAccessor =
        currentTestCaseExecutionContextAccessor;

    public TestCaseExecutionContext? CurrentTestCase
    {
        get => _currentTestCaseExecutionContextAccessor.Context;
        set => _currentTestCaseExecutionContextAccessor.Context = value;
    }
}
