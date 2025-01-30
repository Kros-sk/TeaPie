using Microsoft.Extensions.Logging;
using TeaPie.Reporting;
using TeaPie.StructureExploration;
using TeaPie.TestCases;

namespace TeaPie;

internal class ApplicationContext(
    string path,
    IServiceProvider serviceProvider,
    ICurrentTestCaseExecutionContextAccessor currentTestCaseExecutionContextAccessor,
    ITestsResultsSummaryReporter reporter,
    ILogger logger,
    string tempFolderPath,
    string environment = "",
    string environmentFilePath = "")
{
    public string Path { get; } = path.NormalizePath();
    public string TempFolderPath { get; set; } = tempFolderPath.NormalizePath();

    public string EnvironmentName { get; set; } = environment;
    public string EnvironmentFilePath { get; set; } = environmentFilePath;

    public IReadOnlyCollectionStructure CollectionStructure { get; set; } = new CollectionStructure();
    public IReadOnlyCollection<TestCase> TestCases => CollectionStructure.TestCases;

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

    public ITestsResultsSummaryReporter Reporter { get; } = reporter;
}
