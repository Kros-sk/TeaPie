using Microsoft.Extensions.Logging;
using TeaPie.StructureExploration;
using TeaPie.TestCases;

namespace TeaPie;

internal class ApplicationContext(
    string path,
    ILogger logger,
    IServiceProvider serviceProvider,
    TeaPie userContext,
    string tempFolder = "")
{
    public string Path { get; } = path;
    public string TempFolderPath { get; set; } = tempFolder;

    public IReadOnlyDictionary<string, TestCase> TestCases { get; set; } = new Dictionary<string, TestCase>();
    public Dictionary<string, Script> UserDefinedScripts { get; set; } = [];

    public ILogger Logger { get; set; } = logger;

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public TeaPie UserContext { get; init; } = userContext;

    private TestCaseExecutionContext? _currentTestCase;
    public TestCaseExecutionContext? CurrentTestCase
    {
        get => _currentTestCase;
        set
        {
            _currentTestCase = value;
            UserContext._currentTestCaseExecutionContext = value;
        }
    }
}
