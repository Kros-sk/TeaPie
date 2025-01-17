namespace TeaPie.Variables;

public static class TeaPieVariablesExtensions
{
    /// <summary>
    /// Attempts to retrieve the <b>first matching</b> variable with the specified <paramref name="name"/> of type
    /// <typeparamref name="T"/>. If no such variable is found, the <paramref name="defaultValue"/> is returned.
    /// Variables are searched across all levels in the following order: <b><i>TestCaseVariables, CollectionVariables,
    /// EnvironmentVariables, GlobalVariables</i></b>.
    /// </summary>
    /// <typeparam name="T">The type of the variable to retrieve.</typeparam>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">The name of the variable to retrieve.</param>
    /// <param name="defaultValue">The value to return if no variable with the specified <paramref name="name"/> and
    /// <typeparamref name="T"/> type is found.</param>
    /// <returns>The variable with the specified <paramref name="name"/> of type <typeparamref name="T"/>, or
    /// <paramref name="defaultValue"/> if no matching variable is found.</returns>
    public static T? GetVariable<T>(this TeaPie teaPie, string name, T? defaultValue = default)
        => teaPie._variables.GetVariable(name, defaultValue);

    /// <summary>
    /// Determines whether a variable with the specified <paramref name="name"/> exists.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">The name of the variable to check for existence.</param>
    /// <returns><c>true</c> if a variable with the specified <paramref name="name"/> exists; otherwise, <c>false</c>.</returns>
    public static bool ContainsVariable(this TeaPie teaPie, string name)
        => teaPie._variables.ContainsVariable(name);

    /// <summary>
    /// Stores a variable with the specified <paramref name="name"/> of type <typeparamref name="T"/>
    /// at the <b>Collection level</b>. The variable is tagged with the specified <paramref name="tags"/>, which are optional.
    /// </summary>
    /// <typeparam name="T">The type of the variable to store.</typeparam>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">The name under which the variable will be stored.</param>
    /// <param name="value">The value of the variable to store.</param>
    /// <param name="tags">An optional list of tags associated with the variable.</param>
    public static void SetVariable<T>(this TeaPie teaPie, string name, T value, params string[] tags)
        => teaPie._variables.SetVariable(name, value, tags);

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
