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
