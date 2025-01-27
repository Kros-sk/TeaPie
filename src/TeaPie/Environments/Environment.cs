using TeaPie.Variables;

namespace TeaPie.Environments;

internal class Environment(string name, Dictionary<string, object?> variables)
{
    public string Name { get; set; } = name;
    public IReadOnlyDictionary<string, object?> Variables { get; set; } = GetResolvedVariables(variables);

    private static Dictionary<string, object?> GetResolvedVariables(Dictionary<string, object?> variables)
    {
        Dictionary<string, object?> resolvedVariables = [];
        foreach (var variable in variables)
        {
            resolvedVariables.Add(variable.Key, VariableTypeResolver.Resolve(variable.Value));
        }

        return resolvedVariables;
    }

    public void Apply(VariablesCollection targetCollection)
    {
        foreach (var variable in Variables)
        {
            targetCollection.Set(variable.Key, variable.Value);
        }
    }
}
