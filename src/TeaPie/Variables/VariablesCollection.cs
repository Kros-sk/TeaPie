using System.Collections;
using System.Diagnostics;

namespace TeaPie.Variables;

[DebuggerDisplay("{_variables}")]
public class VariablesCollection : IEnumerable<Variable>
{
    private readonly Dictionary<string, Variable> _variables = [];

    public int Count => _variables.Count;

    public void Set<T>(string variableName, T? value, params string[] tags)
    {
        VariableNameValidator.Resolve(variableName);
        _variables[variableName] = new Variable(variableName, value, tags);
    }

    /// <summary>
    /// Attempts to get variable with given <paramref name="name"/> of <typeparamref name="T"/> type. If no such variable is
    /// found, <paramref name="defaultValue"/> is retrieved.
    /// </summary>
    /// <typeparam name="T">Type of the variable.</typeparam>
    /// <param name="name">Name of the variable.</param>
    /// <param name="defaultValue">Value, that will be retrieved when no variable with given <paramref name="name"/> of
    /// <typeparamref name="T"/> type was found.</param>
    /// <returns>Variable value or <paramref name="defaultValue"/> if no variable with given <paramref name="name"/> of
    /// <typeparamref name="T"/> type was found.</returns>
    public T? Get<T>(string name, T? defaultValue = default)
    {
        try
        {
            var variable = _variables[name];
            return variable.GetValue<T>();
        }
        catch
        {
            return defaultValue;
        }
    }

    public bool Contains(string variableName) => _variables.ContainsKey(variableName);

    public bool Remove(string variableName) => _variables.Remove(variableName);

    /// <summary>
    /// Attempts to remove all variables with given <paramref name="tag"/>. If removal of any of them fails,
    /// <c>false</c> is retrieved.
    /// </summary>
    /// <param name="tag">Tag by which variables are going to be deleted.</param>
    /// <returns>Whether removal of all variables tagged with <paramref name="tag"/> was successfull. If at least one removal
    /// failed, <c>false</c> is returned.</returns>
    public bool RemoveVariablesWithTag(string tag)
    {
        var variablesToDelete = _variables.Where(x => x.Value.HasTag(tag));
        var deleted = variablesToDelete.Any();
        foreach (var item in variablesToDelete)
        {
            deleted = deleted && _variables.Remove(item.Key);
        }
        return deleted;
    }

    public void Clear() => _variables.Clear();

    public IEnumerator<Variable> GetEnumerator() => _variables.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
