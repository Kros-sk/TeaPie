using Microsoft.Extensions.Logging;
using TeaPie.TestCases;
using TeaPie.Testing;
using TeaPie.Variables;

namespace TeaPie;

public sealed class TeaPie : IVariablesExposer, IExecutionContextExposer, ITester
{
    internal static TeaPie Create(IVariables variables, ILogger logger, ITester tester)
    {
        Instance = new(variables, logger, tester);
        return Instance;
    }

    public static TeaPie? Instance { get; private set; }

    private TeaPie(IVariables variables, ILogger logger, ITester tester)
    {
        _variables = variables;
        Logger = logger;
        _tester = tester;
    }

    public ILogger Logger { get; }

    #region Variables
    internal readonly IVariables _variables;
    public VariablesCollection GlobalVariables => _variables.GlobalVariables;
    public VariablesCollection EnvironmentVariables => _variables.EnvironmentVariables;
    public VariablesCollection CollectionVariables => _variables.CollectionVariables;
    public VariablesCollection TestCaseVariables => _variables.TestCaseVariables;
    #endregion

    #region Execution Context
    internal TestCaseExecutionContext? _currentTestCaseExecutionContext;
    public Dictionary<string, HttpRequestMessage> Requests => _currentTestCaseExecutionContext?.Requests ?? [];
    public Dictionary<string, HttpResponseMessage> Responses => _currentTestCaseExecutionContext?.Responses ?? [];
    public HttpRequestMessage? Request => _currentTestCaseExecutionContext?.Request;
    public HttpResponseMessage? Response => _currentTestCaseExecutionContext?.Response;
    #endregion

    #region Testing
    internal ITester _tester;
    public void Test(string testName, Action testFunction) => _tester.Test(testName, testFunction);
    public Task Test(string testName, Func<Task> testFunction) => _tester.Test(testName, testFunction);
    public void AddTestTheory(Action<Theory> testFunction) => _tester.AddTestTheory(testFunction);
    public void AddTestTheory(string testName, Action<Theory> testFunction)
        => _tester.AddTestTheory(testName, testFunction);
    #endregion
}
