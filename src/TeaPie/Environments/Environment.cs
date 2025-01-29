using TeaPie.Variables;

namespace TeaPie.Environments;

internal class Environment(string name, Dictionary<string, object?> variables)
{
    public string Name { get; set; } = name;
    public IReadOnlyDictionary<string, object?> Variables { get; set; } = variables;

    public void Apply(VariablesCollection targetCollection)
    {
        foreach (var variable in Variables)
        {
            targetCollection.Set(variable.Key, variable.Value);
        }
    }
}
