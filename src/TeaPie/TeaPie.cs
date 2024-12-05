using Microsoft.Extensions.Logging;
using TeaPie.TestCases;
using TeaPie.Variables;

namespace TeaPie;

public sealed class TeaPie : IVariablesExposer, IExecutionContextExposer
{
    internal static TeaPie Create(IVariables variables, ILogger logger)
    {
        Instance = new(variables, logger);
        return Instance;
    }

    public static TeaPie? Instance { get; private set; }

    private TeaPie(IVariables variables, ILogger logger)
    {
        _variables = variables;
        Logger = logger;
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
}
