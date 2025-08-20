namespace TeaPie.Functions;

public interface IFunctionsOperations
{
    T? ExecFunction<T>(string name, params object[] args);
    T? ExecFunction<T>(string name);

    bool ContainsFunction(string name);

    void RegisterFunction<T>(string name, Func<T> func);

    void RegisterFunction<T1, T>(string name, Func<T1, T> func);

    void RegisterFunction<T1, T2, T>(string name, Func<T1, T2, T> func);

    bool RemoveFunction(string name);
}
