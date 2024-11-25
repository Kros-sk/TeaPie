namespace TeaPie.Variables;

internal interface IVariablesAccessor
{
    VariablesCollection GlobalVariables { get; }

    VariablesCollection EnvironmentVariables { get; }

    VariablesCollection CollectionVariables { get; }

    VariablesCollection TestCaseVariables { get; }
}
