using Microsoft.Extensions.Logging;
using TeaPie.Environments;
using TeaPie.Http.Auth;
using TeaPie.Http.Retrying;
using TeaPie.Reporting;
using TeaPie.TestCases;
using TeaPie.Testing;
using TeaPie.Variables;

namespace TeaPie;

public sealed class TeaPie : IVariablesExposer, IExecutionContextExposer
{
    public static TeaPie? Instance { get; private set; }

    internal static TeaPie Create(
        ApplicationContext applicationContext,
        IServiceProvider serviceProvider,
        IVariables variables,
        ILogger logger,
        ITester tester,
        ICurrentTestCaseExecutionContextAccessor currentTestCaseExecutionContextAccessor,
        ITestResultsSummaryReporter reporter,
        IRetryStrategyRegistry retryStrategiesRegistry,
        IAuthProviderRegistry authenticationProviderRegistry,
        IAuthProviderAccessor defaultAuthProviderAccessor,
        ITestFactory predefinedTestFactory)
    {
        Instance = new(
            applicationContext,
            serviceProvider,
            variables,
            logger,
            tester,
            currentTestCaseExecutionContextAccessor,
            reporter,
            retryStrategiesRegistry,
            authenticationProviderRegistry,
            defaultAuthProviderAccessor,
            predefinedTestFactory);

        return Instance;
    }

    private TeaPie(
        ApplicationContext applicationContext,
        IServiceProvider serviceProvider,
        IVariables variables,
        ILogger logger,
        ITester tester,
        ICurrentTestCaseExecutionContextAccessor currentTestCaseExecutionContextAccessor,
        ITestResultsSummaryReporter reporter,
        IRetryStrategyRegistry retryStrategiesRegistry,
        IAuthProviderRegistry authenticationProviderRegistry,
        IAuthProviderAccessor defaultAuthProviderAccessor,
        ITestFactory predefinedTestFactory)
    {
        _applicationContext = applicationContext;
        _serviceProvider = serviceProvider;

        _variables = variables;
        Logger = logger;
        _tester = tester;
        _currentTestCaseExecutionContextAccessor = currentTestCaseExecutionContextAccessor;
        _reporter = reporter;
        _retryStrategyRegistry = retryStrategiesRegistry;
        _authenticationProviderRegistry = authenticationProviderRegistry;
        _defaultAuthProviderAccessor = defaultAuthProviderAccessor;
        _testFactory = predefinedTestFactory;
    }

    internal IServiceProvider _serviceProvider;

    private readonly ApplicationContext _applicationContext;
    public IApplicationContext ApplicationContext => _applicationContext;

    #region Logging
    public ILogger Logger { get; }
    #endregion

    #region Variables
    internal readonly IVariables _variables;
    public VariablesCollection GlobalVariables => _variables.GlobalVariables;
    public VariablesCollection EnvironmentVariables => _variables.EnvironmentVariables;
    public VariablesCollection CollectionVariables => _variables.CollectionVariables;
    public VariablesCollection TestCaseVariables => _variables.TestCaseVariables;

    /// <summary>
    /// Attempts to retrieve the <b>first matching</b> variable with the specified <paramref name="name"/> of type
    /// <typeparamref name="T"/>. If no such variable is found, the <paramref name="defaultValue"/> is returned.
    /// Variables are searched across all levels in the following order: <b><i>TestCaseVariables, CollectionVariables,
    /// EnvironmentVariables, GlobalVariables</i></b>.
    /// </summary>
    /// <typeparam name="T">The type of the variable to retrieve.</typeparam>
    /// <param name="name">The name of the variable to retrieve.</param>
    /// <param name="defaultValue">The value to return if no variable with the specified <paramref name="name"/> and
    /// <typeparamref name="T"/> type is found.</param>
    /// <returns>The variable with the specified <paramref name="name"/> of type <typeparamref name="T"/>, or
    /// <paramref name="defaultValue"/> if no matching variable is found.</returns>
    public T? GetVariable<T>(string name, T? defaultValue = default)
        => _variables.GetVariable(name, defaultValue);

    /// <summary>
    /// Stores a variable with the specified <paramref name="name"/> of type <typeparamref name="T"/>
    /// at the <b>Collection level</b>. The variable is tagged with the specified <paramref name="tags"/>, which are optional.
    /// </summary>
    /// <typeparam name="T">The type of the variable to store.</typeparam>
    /// <param name="name">The name under which the variable will be stored.</param>
    /// <param name="value">The value of the variable to store.</param>
    /// <param name="tags">An optional list of tags associated with the variable.</param>
    public void SetVariable<T>(string name, T value, params string[] tags)
        => _variables.SetVariable(name, value, tags);
    #endregion

    #region Execution Context
    private readonly ICurrentTestCaseExecutionContextAccessor _currentTestCaseExecutionContextAccessor;

    internal TestCaseExecutionContext? CurrentTestCaseExecutionContext
        => _currentTestCaseExecutionContextAccessor.Context;

    /// <summary>
    /// Collection of current test case's HTTP requests accessible by names.
    /// </summary>
    public IReadOnlyDictionary<string, HttpRequestMessage> Requests =>
        CurrentTestCaseExecutionContext?.Requests ?? new Dictionary<string, HttpRequestMessage>();

    /// <summary>
    /// Collection of current test case's HTTP responses accessible by names.
    /// </summary>
    public IReadOnlyDictionary<string, HttpResponseMessage> Responses =>
        CurrentTestCaseExecutionContext?.Responses ?? new Dictionary<string, HttpResponseMessage>();

    /// <summary>
    /// The most recently executed HTTP request.
    /// </summary>
    public HttpRequestMessage? Request => CurrentTestCaseExecutionContext?.Request;

    /// <summary>
    /// The most recently retrieved HTTP response.
    /// </summary>
    public HttpResponseMessage? Response => CurrentTestCaseExecutionContext?.Response;
    #endregion

    #region Testing
    internal readonly ITester _tester;
    internal readonly ITestFactory _testFactory;
    #endregion

    #region Environments
    /// <summary>
    /// Set environment to one with given <paramref name="name"/>. Environment <b>must be defined in the environment file</b>.
    /// </summary>
    /// <param name="name">Name of the environment to be set.</param>
    public void SetEnvironment(string name)
    {
        _applicationContext.EnvironmentName = name;
        Task.Run(() => _applicationContext.ServiceProvider.GetStep<SetEnvironmentStep>().Execute(_applicationContext));
    }
    #endregion

    #region Reporting
    internal readonly ITestResultsSummaryReporter _reporter;
    #endregion

    #region Re-trying
    internal readonly IRetryStrategyRegistry _retryStrategyRegistry;
    #endregion

    #region Authentication
    internal readonly IAuthProviderRegistry _authenticationProviderRegistry;
    internal readonly IAuthProviderAccessor _defaultAuthProviderAccessor;
    #endregion
}
