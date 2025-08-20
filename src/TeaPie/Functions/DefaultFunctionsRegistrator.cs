namespace TeaPie.Functions;
internal static class DefaultFunctionsRegistrator
{
    public static void Register(FunctionsCollection defaultFunctions)
    {
        defaultFunctions.Set("$now", (string? format) => DateTime.Now.ToString(format));
        defaultFunctions.Set("$guid", Guid.NewGuid);
    }
}
