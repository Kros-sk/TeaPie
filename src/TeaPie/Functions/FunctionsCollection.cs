using System.Collections;
using System.Diagnostics;

namespace TeaPie.Functions;

[DebuggerDisplay("{_functions}")]
internal class FunctionsCollection : IEnumerable<Function>
{
    private readonly Dictionary<string, Function> _functions = [];

    public int Count => _functions.Count;

    public void Register<TResult>(string name, Func<TResult> func)
    {
        FunctionNameValidator.Resolve(name);
        string key = GenerateFunctionNameKey(name, 0);
        _functions[key] = new Function<TResult>(name, func);
    }

    public void Register<TParameter1, TResult>(string name, Func<TParameter1, TResult> func)
    {
        FunctionNameValidator.Resolve(name);
        string key = GenerateFunctionNameKey(name, 1);
        _functions[key] = new Function<TParameter1, TResult>(name, func);
    }

    public void Register<TParameter1, TParameter2, TResult>(string name, Func<TParameter1, TParameter2, TResult> func)
    {
        FunctionNameValidator.Resolve(name);
        string key = GenerateFunctionNameKey(name, 2);
        _functions[key] = new Function<TParameter1, TParameter2, TResult>(name, func);
    }

    public TResult? Execute<TResult>(string name, params object[] args)
    {
        var function = _functions[GenerateFunctionNameKey(name, args.Length)];
        return (TResult?)function.InvokeFunction(args);
    }

    public TResult? Execute<TResult>(string name)
    {
        var function = _functions[GenerateFunctionNameKey(name, 0)];
        return (TResult?)function.InvokeFunction(null);
    }

    public bool Contains(string name, int argsCount) => _functions.ContainsKey(GenerateFunctionNameKey(name, argsCount));

    public IEnumerator<Function> GetEnumerator() => _functions.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static string GenerateFunctionNameKey(string name, int argsCount)
        => $"{name}_({argsCount})";
}
