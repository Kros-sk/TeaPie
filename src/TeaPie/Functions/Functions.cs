namespace TeaPie.Functions;

internal interface IFunctions : IFunctionsOperations, IFunctionsExposer;

internal class Functions : IFunctions
{
    private FunctionsCollection DefaultFunctions { get; } = [];
    public FunctionsCollection CustomFunctions { get; } = [];

    public Functions()
    {
        DefaultFunctionsRegistrator.Register(DefaultFunctions);
    }

    private IEnumerable<FunctionsCollection> GetAllFunctions()
        => [DefaultFunctions, CustomFunctions];

    public T? Execute<T>(string name, params object[] args)
    {
        var collectionWithFunction = GetCollectionWithFunction(name, args.Length);
        return collectionWithFunction is not null ? collectionWithFunction.Execute<T>(name, args) : default;
    }

    public T? Execute<T>(string name)
    {
        var collectionWithFunction = GetCollectionWithFunction(name, 0);
        return collectionWithFunction is not null ? collectionWithFunction.Execute<T>(name) : default;
    }

    private FunctionsCollection? GetCollectionWithFunction(string name, int argsCount)
        => GetAllFunctions().FirstOrDefault(vc => vc.Contains(name, argsCount));

    public bool Contains(string name, int argsCount) => GetAllFunctions().Any(vc => vc.Contains(name, argsCount));

    public void Register<T>(string name, Func<T> func)
    {
        if (!DefaultFunctions.Contains(name, 0))
        {
            CustomFunctions.Register(name, func);
        }
    }

    public void Register<T1, T>(string name, Func<T1, T> func)
    {
        if (!DefaultFunctions.Contains(name, 1))
        {
            CustomFunctions.Register(name, func);
        }
    }

    public void Register<T1, T2, T>(string name, Func<T1, T2, T> func)
    {
        if (!DefaultFunctions.Contains(name, 2))
        {
            CustomFunctions.Register(name, func);
        }
    }
}
