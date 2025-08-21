namespace TeaPie.Functions;

internal abstract class Function(string name)
{
    public string Name { get; set; } = name;
    public abstract object? InvokeFunction(params object[]? args);
}

internal class Function<T>(string name, Func<T> func) : Function(name)
{
    private readonly Func<T> _func = func;
    public override object? InvokeFunction(params object[]? args) => _func();
}

internal class Function<T1, T>(string name, Func<T1, T> func) : Function(name)
{
    private readonly Func<T1, T> _func = func;
    public override object? InvokeFunction(params object[]? args)
    {
        var arg = (args?.FirstOrDefault()) ?? throw new ArgumentException($"Not enough arguments for function {Name}.");
        return _func.Invoke((T1)Convert.ChangeType(arg, typeof(T1)));
    }
}

internal class Function<T1, T2, T>(string name, Func<T1, T2, T> func) : Function(name)
{
    private readonly Func<T1, T2, T> _func = func;
    public override object? InvokeFunction(params object[]? args)
    {
        if (args is null || args.Length < 2)
        {
            throw new ArgumentException($"Not enough arguments for function {Name}.");
        }

        return _func((T1)Convert.ChangeType(args[0], typeof(T1)), (T2)Convert.ChangeType(args[1], typeof(T2)));
    }
}
