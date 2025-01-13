using Microsoft.Extensions.Logging;
using TeaPie.TestCases;
using TeaPie.Testing;
using TeaPie.Variables;

namespace TeaPie;

public sealed class TeaPie : IVariablesExposer, IExecutionContextExposer
{
    internal static TeaPie Create(
        IVariables variables,
        ILogger logger,
        ITester tester,
        ICurrentTestCaseExecutionContextAccessor currentTestCaseExecutionContextAccessor)
    {
        Instance = new(variables, logger, tester, currentTestCaseExecutionContextAccessor);
        return Instance;
    }

    public static TeaPie? Instance { get; private set; }

    private TeaPie(
        IVariables variables,
        ILogger logger,
        ITester tester,
        ICurrentTestCaseExecutionContextAccessor currentTestCaseExecutionContextAccessor)
    {
        _variables = variables;
        Logger = logger;
        _tester = tester;
        _currentTestCaseExecutionContextAccessor = currentTestCaseExecutionContextAccessor;
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
    private readonly ICurrentTestCaseExecutionContextAccessor _currentTestCaseExecutionContextAccessor;

    internal TestCaseExecutionContext? CurrentTestCaseExecutionContext
        => _currentTestCaseExecutionContextAccessor.Context;

    /// <summary>
    /// Collection of current test case's http requests accessible by names.
    /// </summary>
    public IReadOnlyDictionary<string, HttpRequestMessage> Requests =>
        CurrentTestCaseExecutionContext?.Requests ?? new Dictionary<string, HttpRequestMessage>();

    /// <summary>
    /// Collection of current test case's http responses accessible by names.
    /// </summary>
    public IReadOnlyDictionary<string, HttpResponseMessage> Responses =>
        CurrentTestCaseExecutionContext?.Responses ?? new Dictionary<string, HttpResponseMessage>();

    /// <summary>
    /// The most recently executed http request.
    /// </summary>
    public HttpRequestMessage? Request => CurrentTestCaseExecutionContext?.Request;

    /// <summary>
    /// The most recently retrieved http response.
    /// </summary>
    public HttpResponseMessage? Response => CurrentTestCaseExecutionContext?.Response;
    #endregion

    #region Testing
    internal readonly ITester _tester;
    #endregion
}
