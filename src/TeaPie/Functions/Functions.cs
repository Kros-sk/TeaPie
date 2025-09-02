namespace TeaPie.Functions;

internal interface IFunctions : IFunctionsOperations;

internal class Functions : IFunctions
{
    private FunctionsCollection DefaultFunctions { get; } = [];
    private FunctionsCollection CustomFunctions { get; } = [];

    public Functions()
    {
        DefaultFunctionsRegistrator.Register(DefaultFunctions);
    }

    private IEnumerable<FunctionsCollection> GetAllFunctions()
        => [DefaultFunctions, CustomFunctions];

    public TResult? Execute<TResult>(string name, params object[] args)
    {
        var collectionWithFunction = GetCollectionWithFunction(name, args.Length);
        return collectionWithFunction is not null ? collectionWithFunction.Execute<TResult>(name, args) : default;
    }

    public TResult? Execute<TResult>(string name)
    {
        var collectionWithFunction = GetCollectionWithFunction(name, 0);
        return collectionWithFunction is not null ? collectionWithFunction.Execute<TResult>(name) : default;
    }

    private FunctionsCollection? GetCollectionWithFunction(string name, int argsCount)
        => GetAllFunctions().FirstOrDefault(vc => vc.Contains(name, argsCount));

    public bool Contains(string name, int argsCount) => GetAllFunctions().Any(vc => vc.Contains(name, argsCount));

    public void Register<TResult>(string name, Func<TResult> func)
    {
        if (!DefaultFunctions.Contains(name, 0))
        {
            CustomFunctions.Register(name, func);
        }
    }

    public void Register<TParameter1, TResult>(string name, Func<TParameter1, TResult> func)
    {
        if (!DefaultFunctions.Contains(name, 1))
        {
            CustomFunctions.Register(name, func);
        }
    }

    public void Register<TParameter1, TParameter2, TResult>(string name, Func<TParameter1, TParameter2, TResult> func)
    {
        if (!DefaultFunctions.Contains(name, 2))
        {
            CustomFunctions.Register(name, func);
        }
    }
}
