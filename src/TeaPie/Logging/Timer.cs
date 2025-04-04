using System.Diagnostics;

namespace TeaPie.Logging;

internal static class Timer
{
    public static async Task<T> Execute<T>(Func<Task<T>> asyncFunction, Action<long> log)
    {
        var stopwatch = Stopwatch.StartNew();

        var result = await asyncFunction();

        stopwatch.Stop();
        log(stopwatch.ElapsedMilliseconds);
        return result;
    }

    public static T Execute<T>(Func<T> function, Action<long> log)
    {
        var stopwatch = Stopwatch.StartNew();

        var result = function();

        stopwatch.Stop();
        log(stopwatch.ElapsedMilliseconds);
        return result;
    }

    public static void Execute(Action action, Action<long> log)
    {
        var stopwatch = Stopwatch.StartNew();

        action();

        stopwatch.Stop();
        log(stopwatch.ElapsedMilliseconds);
    }
}
