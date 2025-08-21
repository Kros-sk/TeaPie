namespace TeaPie.Functions;

public interface IFunctionsOperations
{
    T? Execute<T>(string name, params object[] args);
    T? Execute<T>(string name);

    bool Contains(string name);

    void Register<T>(string name, Func<T> func);

    void Register<T1, T>(string name, Func<T1, T> func);

    void Register<T1, T2, T>(string name, Func<T1, T2, T> func);
}
