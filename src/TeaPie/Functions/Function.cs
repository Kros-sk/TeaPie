namespace TeaPie.Functions;

internal abstract class Function(string name)
{
    public string Name { get; set; } = name;
    public abstract object? InvokeFunction(params object[]? args);
}

internal class Function<TResult>(string name, Func<TResult> func) : Function(name)
{
    private readonly Func<TResult> _func = func;
    public override object? InvokeFunction(params object[]? args) => _func();
}

internal class Function<TParameter1, TResult>(string name, Func<TParameter1, TResult> func) : Function(name)
{
    private readonly Func<TParameter1, TResult> _func = func;
    public override object? InvokeFunction(params object[]? args)
    {
        var arg = (args?.FirstOrDefault()) ?? throw new ArgumentException($"Not enough arguments for function {Name}.");
        return _func.Invoke((TParameter1)Convert.ChangeType(arg, typeof(TParameter1)));
    }
}

internal class Function<TParameter1, TParameter2, TResult>(
    string name,
    Func<TParameter1, TParameter2, TResult> func) : Function(name)
{
    private readonly Func<TParameter1, TParameter2, TResult> _func = func;
    public override object? InvokeFunction(params object[]? args)
    {
        if (args is null || args.Length < 2)
        {
            throw new ArgumentException($"Not enough arguments for function {Name}.");
        }

        return _func(
            (TParameter1)Convert.ChangeType(args[0], typeof(TParameter1)),
            (TParameter2)Convert.ChangeType(args[1], typeof(TParameter2)));
    }
}
