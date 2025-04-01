using Microsoft.Extensions.Logging;
using TeaPie.Reporting;
using TeaPie.StructureExploration;
using TeaPie.StructureExploration.Paths;
using TeaPie.TestCases;

namespace TeaPie;

internal class ApplicationContext(
    string path,
    IServiceProvider serviceProvider,
    ICurrentTestCaseExecutionContextAccessor currentTestCaseExecutionContextAccessor,
    ITestResultsSummaryReporter reporter,
    ILogger<ApplicationContext> logger,
    ApplicationContextOptions options) : IApplicationContext
{
    public string Path { get; } = path.NormalizePath();

    public string TempFolderPath = options.TempFolderPath;

    public string EnvironmentName { get; set; } = options.Environment;
    public string EnvironmentFilePath { get; set; } = options.EnvironmentFilePath;

    public readonly string ReportFilePath = options.ReportFilePath;

    public string InitializationScriptPath = options.InitializationScriptPath;

    public bool CacheVariables = options.CacheVariables;

    public string StructureName => System.IO.Path.GetFileNameWithoutExtension(Path).TrimSuffix(Constants.RequestSuffix);

    public string TeaPieFolderPath { get; internal set; } = string.Empty;

    public IReadOnlyCollectionStructure CollectionStructure { get; set; } = new CollectionStructure();
    public IReadOnlyCollection<TestCase> TestCases => CollectionStructure.TestCases;

    private readonly Dictionary<string, Script> _userDefinedScripts = [];
    public IReadOnlyDictionary<string, Script> UserDefinedScripts => _userDefinedScripts;
    public void RegisterUserDefinedScript(string path, Script script) => _userDefinedScripts.Add(path, script);

    public ILogger Logger { get; set; } = logger;

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    private readonly ICurrentTestCaseExecutionContextAccessor _currentTestCaseExecutionContextAccessor =
        currentTestCaseExecutionContextAccessor;

    public TestCaseExecutionContext? CurrentTestCase
    {
        get => _currentTestCaseExecutionContextAccessor.Context;
        set => _currentTestCaseExecutionContextAccessor.Context = value;
    }

    public ITestResultsSummaryReporter Reporter { get; } = reporter;

    public PrematureTermination? PrematureTermination { get; set; }
}
