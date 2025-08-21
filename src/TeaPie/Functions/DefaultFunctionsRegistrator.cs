namespace TeaPie.Functions;

internal static class DefaultFunctionsRegistrator
{
    public static void Register(FunctionsCollection defaultFunctions)
    {
        defaultFunctions.Register("$now", (string? format) => DateTime.Now.ToString(format));
        defaultFunctions.Register("$guid", Guid.NewGuid);
        defaultFunctions.Register("$rand", Random.Shared.NextDouble);
        defaultFunctions.Register("$randomInt", (int min, int max) => Random.Shared.Next(min, max));
    }
}
