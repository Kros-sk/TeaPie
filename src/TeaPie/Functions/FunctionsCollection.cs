using System.Collections;
using System.Diagnostics;

namespace TeaPie.Functions;

[DebuggerDisplay("{_functions}")]
public class FunctionsCollection : IEnumerable<Function>
{
    private readonly Dictionary<string, Function> _functions = [];

    public int Count => _functions.Count;

    public void Register<T>(string name, Func<T> func)
    {
        FunctionNameValidator.Resolve(name);
        string key = GenerateFunctionNameKey(name, 0);
        _functions[key] = new Function<T>(name, func);
    }

    public void Register<T, T1>(string name, Func<T, T1> func)
    {
        FunctionNameValidator.Resolve(name);
        string key = GenerateFunctionNameKey(name, 1);
        _functions[key] = new Function<T, T1>(name, func);
    }

    public void Register<T, T1, T2>(string name, Func<T, T1, T2> func)
    {
        FunctionNameValidator.Resolve(name);
        string key = GenerateFunctionNameKey(name, 2);
        _functions[key] = new Function<T, T1, T2>(name, func);
    }

    /// <summary>
    /// Attempts to run function with given <paramref name="name"/> of <typeparamref name="T"/> type. If no such function is
    /// found, default is retrieved.
    /// </summary>
    /// <typeparam name="T">Type of the result of the function.</typeparam>
    /// <param name="name">Name of the function.</param>
    /// <param name="args">Function argumetns</param>
    /// <returns>Function result or default if no function with given <paramref name="name"/> of type was found.</returns>
    public T? Execute<T>(string name, params object[] args)
    {
        var function = _functions[GenerateFunctionNameKey(name, args.Length)];
        return (T?)function.InvokeFunction(args);
    }

    public T? Execute<T>(string name)
    {
        var function = _functions[GenerateFunctionNameKey(name, 0)];
        return (T?)function.InvokeFunction(null);
    }

    public bool Contains(string name, int argsCount) => _functions.ContainsKey(GenerateFunctionNameKey(name, argsCount));

    public IEnumerator<Function> GetEnumerator() => _functions.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static string GenerateFunctionNameKey(string name, int argsCount)
        => $"{name}_({argsCount})";
}
