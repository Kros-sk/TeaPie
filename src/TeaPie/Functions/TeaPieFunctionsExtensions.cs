namespace TeaPie.Functions;

public static class TeaPieFunctionsExtensions
{
    /// <summary>
    /// Register a function with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">The name under which the function will be registered.</param>
    /// <param name="func">Predicate of registered function.</param>
    public static void RegisterFunction<TResult>(this TeaPie teaPie, string name, Func<TResult> func)
        => teaPie._functions.Register(name, func);

    /// <summary>
    /// Register a function with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">The name under which the function will be registered.</param>
    /// <param name="func">Predicate of registered function.</param>
    public static void RegisterFunction<TParameter1, TResult>(this TeaPie teaPie, string name, Func<TParameter1, TResult> func)
       => teaPie._functions.Register(name, func);

    /// <summary>
    /// Register a function with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">The name under which the function will be registered.</param>
    /// <param name="func">Predicate of registered function.</param>
    public static void RegisterFunction<TParameter1, TParameter2, TResult>(this TeaPie teaPie, string name, Func<TParameter1, TParameter2, TResult> func)
        => teaPie._functions.Register(name, func);

    /// <summary>
    /// Executes a function with the specified <paramref name="name"/> and returns its result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result of the function to retrieve.</typeparam>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">The name of the function to retrieve.</param>
    /// <param name="args">Function's arguments.</param>
    public static TResult? ExecFunction<TResult>(this TeaPie teaPie, string name, params object[] args)
        => teaPie._functions.Execute<TResult>(name, args);

    /// <summary>
    /// Executes a function with the specified <paramref name="name"/> and returns its result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result of the function to retrieve.</typeparam>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="name">The name of the function to retrieve.</param>
    public static TResult? ExecFunction<TResult>(this TeaPie teaPie, string name)
       => teaPie._functions.Execute<TResult>(name);
}
