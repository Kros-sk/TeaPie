namespace TeaPie.Variables;

internal interface IVariables : IVariablesOperations, IVariablesExposer;

internal class Variables : IVariables
{
    public VariablesCollection GlobalVariables { get; } = [];
    public VariablesCollection EnvironmentVariables { get; } = [];
    public VariablesCollection CollectionVariables { get; } = [];
    public VariablesCollection TestCaseVariables { get; } = [];

    private IEnumerable<VariablesCollection> GetAllVariables()
        => [TestCaseVariables, CollectionVariables, EnvironmentVariables, GlobalVariables];

    public T? GetVariable<T>(string name, T? defaultValue = default)
    {
        var collectionWithVariable = GetAllVariables().FirstOrDefault(vc => vc.Contains(name));
        return collectionWithVariable is not null ? collectionWithVariable.Get(name, defaultValue) : defaultValue;
    }

    public bool ContainsVariable(string name) => GetAllVariables().Any(vc => vc.Contains(name));

    public void SetVariable<T>(string name, T value, params string[] tags)
        => CollectionVariables.Set(name, value, tags);

    public bool RemoveVariable(string name)
        => Remove(name, (coll, name) => coll.Contains(name), (coll, name) => coll.Remove(name));

    public bool RemoveVariablesWithTag(string tag)
        => Remove(tag, (coll, tag) => coll.Any(v => v.HasTag(tag)), (coll, tag) => coll.RemoveVariablesWithTag(tag));

    private bool Remove(
        string key,
        Func<VariablesCollection, string, bool> containsFunction,
        Func<VariablesCollection, string, bool> removalFunction)
    {
        var found = false;
        foreach (var variableCollection in GetAllVariables())
        {
            if (containsFunction(variableCollection, key))
            {
                found = true;
                if (!removalFunction(variableCollection, key))
                {
                    return false;
                }
            }
        }

        return found;
    }
}
