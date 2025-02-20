using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeaPie.Reporting;
using TeaPie.StructureExploration;
using TeaPie.TestCases;

namespace TeaPie;

internal class ApplicationContext(
    string path, IServiceProvider serviceProvider, ApplicationContextOptions options) : IApplicationContext
{
    public string Path { get; } = path.NormalizePath();

    public string TempFolderPath = options.TempFolderPath;

    public string EnvironmentName { get; set; } = options.Environment;
    public string EnvironmentFilePath { get; set; } = options.EnvironmentFilePath;

    public readonly string ReportFilePath = options.ReportFilePath;

    public string InitializationScriptPath = options.InitializationScriptPath;

    public string CollectionName => System.IO.Path.GetFileName(Path);

    public IReadOnlyCollectionStructure CollectionStructure { get; set; } = new CollectionStructure();
    public IReadOnlyCollection<TestCase> TestCases => CollectionStructure.TestCases;

    private readonly Dictionary<string, Script> _userDefinedScripts = [];
    public IReadOnlyDictionary<string, Script> UserDefinedScripts => _userDefinedScripts;
    public void RegisterUserDefinedScript(string key, Script script) => _userDefinedScripts.Add(key, script);

    public ILogger Logger { get; set; } = serviceProvider.GetRequiredService<ILogger<ApplicationContext>>();

    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    private readonly ICurrentTestCaseExecutionContextAccessor _currentTestCaseExecutionContextAccessor =
         serviceProvider.GetRequiredService<ICurrentTestCaseExecutionContextAccessor>();

    public TestCaseExecutionContext? CurrentTestCase
    {
        get => _currentTestCaseExecutionContextAccessor.Context;
        set => _currentTestCaseExecutionContextAccessor.Context = value;
    }

    public ITestResultsSummaryReporter Reporter { get; } = serviceProvider.GetRequiredService<ITestResultsSummaryReporter>();
}
