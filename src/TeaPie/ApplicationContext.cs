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
    public Dictionary<string, Script> UserDefinedScripts { get; set; } = [];

    public ILogger Logger { get; set; } = logger;

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    private readonly ICurrentTestCaseExecutionContextAccessor _currentTestCaseExecutionContextAccessor =
        currentTestCaseExecutionContextAccessor;

    public TestCaseExecutionContext? CurrentTestCase
    {
        get => _currentTestCaseExecutionContextAccessor.CurrentTestCaseExecutionContext;
        set => _currentTestCaseExecutionContextAccessor.CurrentTestCaseExecutionContext = value;
    }
}
