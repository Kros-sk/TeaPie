namespace TeaPie.Variables;

internal interface IVariablesExposer
{
    VariablesCollection GlobalVariables { get; }
    VariablesCollection EnvironmentVariables { get; }
    VariablesCollection CollectionVariables { get; }
    VariablesCollection TestCaseVariables { get; }
}
