using System.Collections;
using System.Diagnostics;

namespace TeaPie.Variables;

[DebuggerDisplay("{_variables}")]
internal class VariablesCollection : IEnumerable<Variable>
{
    private readonly Dictionary<string, Variable> _variables = [];

    public int Count => _variables.Count;

    public void Set<T>(string variableName, T? value, params string[] tags)
    {
        VariableNameValidator.Resolve(variableName);
        _variables[variableName] = new Variable(variableName, value, tags);
    }

    public T? Get<T>(string key, T? defaultValue = default)
    {
        try
        {
            var variable = _variables[key];
            return variable.GetValue<T>();
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    public bool Contains(string variableName) => _variables.ContainsKey(variableName);

    public bool Remove(string variableName) => _variables.Remove(variableName);

    public bool RemoveVariables(string tag)
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
