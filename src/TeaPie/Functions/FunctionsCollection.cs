using System.Collections;
using System.Diagnostics;

namespace TeaPie.Functions;

[DebuggerDisplay("{_functions}")]
public class FunctionsCollection : IEnumerable<Function>
{
    private readonly Dictionary<string, Function> _functions = [];

    public int Count => _functions.Count;

    public void Set<T>(string functionName, Func<T> func)
    {
        FunctionNameValidator.Resolve(functionName);
        _functions[functionName] = new Function<T>(functionName, func);
    }

    public void Set<T, T1>(string functionName, Func<T, T1> func)
    {
        FunctionNameValidator.Resolve(functionName);
        _functions[functionName] = new Function<T, T1>(functionName, func);
    }

    public void Set<T, T1, T2>(string functionName, Func<T, T1, T2> func)
    {
        FunctionNameValidator.Resolve(functionName);
        _functions[functionName] = new Function<T, T1, T2>(functionName, func);
    }

    /// <summary>
    /// Attempts to run function with given <paramref name="name"/> of <typeparamref name="T"/> type. If no such function is
    /// found, default is retrieved.
    /// </summary>
    /// <typeparam name="T">Type of the result of the function.</typeparam>
    /// <param name="name">Name of the function.</param>
    /// <param name="args">Function argumetns</param>
    /// <returns>Function result or default if no function with given <paramref name="name"/> of type was found.</returns>
    public T? ExecFunction<T>(string name, params object[] args)
    {
        var function = _functions[name];
        return (T?)function.InvokeFunction(args);
    }

    public T? ExecFunction<T>(string name)
    {
        var function = _functions[name];
        return (T?)function.InvokeFunction(null);
    }

    public bool Contains(string functionName) => _functions.ContainsKey(functionName);

    public bool Remove(string functionName) => _functions.Remove(functionName);

    public void Clear() => _functions.Clear();

    public IEnumerator<Function> GetEnumerator() => _functions.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
