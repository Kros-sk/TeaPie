namespace TeaPie.Functions;

internal interface IFunctionsOperations
{
    TResult? Execute<TResult>(string name, params object[] args);

    TResult? Execute<TResult>(string name);

    bool Contains(string name, int argsCount);

    void Register<TResult>(string name, Func<TResult> func);

    void Register<TParameter1, TResult>(string name, Func<TParameter1, TResult> func);

    void Register<TParameter1, TParameter2, TResult>(string name, Func<TParameter1, TParameter2, TResult> func);
}
