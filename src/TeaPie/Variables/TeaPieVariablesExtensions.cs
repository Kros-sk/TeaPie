namespace TeaPie.Variables;

public static class TeaPieVariablesExtensions
{
    /// <summary>
    /// Determines whether a variable with the specified <paramref name="name"/> exists.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">The name of the variable to check for existence.</param>
    /// <returns><c>true</c> if a variable with the specified <paramref name="name"/> exists; otherwise, <c>false</c>.</returns>
    public static bool ContainsVariable(this TeaPie teaPie, string name)
        => teaPie._variables.ContainsVariable(name);

    /// <summary>
    /// Attempts to remove the variable(s) with the specified <paramref name="name"/> from all levels
    /// (<b><i>TestCaseVariables, CollectionVariables, EnvironmentVariables, GlobalVariables</i></b>).
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">The name of the variable(s) to remove.</param>
    /// <returns><c>true</c> if the variable(s) were successfully removed from all levels; otherwise, <c>false</c>.</returns>
    public static bool RemoveVariable(this TeaPie teaPie, string name)
        => teaPie._variables.RemoveVariable(name);

    /// <summary>
    /// Attempts to remove all variables tagged with the specified <paramref name="tag"/> from all levels
    /// (<b><i>TestCaseVariables, CollectionVariables, EnvironmentVariables, GlobalVariables</i></b>).
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="tag">The tag used to identify variables for removal.</param>
    /// <returns><c>true</c> if all variables with the specified tag were successfully removed from all levels;
    /// otherwise, <c>false</c>.</returns>
    public static bool RemoveVariablesWithTag(this TeaPie teaPie, string tag)
        => teaPie._variables.RemoveVariablesWithTag(tag);
}
