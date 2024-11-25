namespace TeaPie.Variables;

public static class TeaPieVariablesExtensions
{
    public static T? GetVariable<T>(this TeaPie teaPie, string name, T? defaultValue = default)
        => teaPie._variables.GetVariable(name, defaultValue);

    public static bool ContainsVariable(this TeaPie teaPie, string name)
        => teaPie._variables.ContainsVariable(name);

    public static void SetVariable<T>(this TeaPie teaPie, string name, T value, params string[] tags)
        => teaPie._variables.SetVariable(name, value, tags);

    public static bool RemoveVariable(this TeaPie teaPie, string name)
        => teaPie._variables.RemoveVariable(name);

    public static bool RemoveVariablesWithTag(this TeaPie teaPie, string tag)
        => teaPie._variables.RemoveVariablesWithTag(tag);
}
