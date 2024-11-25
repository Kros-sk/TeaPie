namespace TeaPie.Variables;

public interface IVariablesOperations
{
    T? GetVariable<T>(string name, T? defaultValue = default);

    bool ContainsVariable(string name);

    void SetVariable<T>(string name, T value, params string[] tags);

    bool RemoveVariable(string name);

    bool RemoveVariablesWithTag(string tag);
}
