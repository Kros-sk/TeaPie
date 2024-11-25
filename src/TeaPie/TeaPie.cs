using Microsoft.Extensions.Logging;
using TeaPie.Variables;

namespace TeaPie;

public sealed class TeaPie : IVariablesAccessor
{
    internal static TeaPie Create(IVariables variables, ILogger logger)
        => Instance ??= new(variables, logger);

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
}
