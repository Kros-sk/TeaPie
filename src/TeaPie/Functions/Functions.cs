namespace TeaPie.Functions;

internal interface IFunctions : IFunctionsOperations, IFunctionsExposer;

internal class Functions : IFunctions
{
    public FunctionsCollection DefaultFunctions { get; } = [];
    public FunctionsCollection CustomFunctions { get; } = [];

    public Functions()
    {
        DefaultFunctionsRegistrator.Register(DefaultFunctions);
    }

    private IEnumerable<FunctionsCollection> GetAllFunctions()
        => [DefaultFunctions, CustomFunctions];

    public T? ExecFunction<T>(string name, params object[] args)
    {
        var collectionWithFunction = GetCollectionWithFunction(name);
        return collectionWithFunction is not null ? collectionWithFunction.ExecFunction<T>(name, args) : default;
    }

    public T? ExecFunction<T>(string name)
    {
        var collectionWithFunction = GetCollectionWithFunction(name);
        return collectionWithFunction is not null ? collectionWithFunction.ExecFunction<T>(name) : default;
    }

    private FunctionsCollection? GetCollectionWithFunction(string name)
        => GetAllFunctions().FirstOrDefault(vc => vc.Contains(name));

    public bool ContainsFunction(string name) => GetAllFunctions().Any(vc => vc.Contains(name));

    public void RegisterFunction<T>(string name, Func<T> func)
        => CustomFunctions.Set(name, func);

    public void RegisterFunction<T1, T>(string name, Func<T1, T> func)
        => CustomFunctions.Set(name, func);

    public void RegisterFunction<T1, T2, T>(string name, Func<T1, T2, T> func)
        => CustomFunctions.Set(name, func);

    public bool RemoveFunction(string name)
        => Remove(name, (coll, name) => coll.Contains(name), (coll, name) => coll.Remove(name));

    private bool Remove(
        string key,
        Func<FunctionsCollection, string, bool> containsFunction,
        Func<FunctionsCollection, string, bool> removalFunction)
    {
        var found = false;
        foreach (var fucntionCollection in GetAllFunctions())
        {
            if (containsFunction(fucntionCollection, key))
            {
                found = true;
                if (!removalFunction(fucntionCollection, key))
                {
                    return false;
                }
            }
        }

        return found;
    }
}
