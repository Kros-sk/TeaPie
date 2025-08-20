namespace TeaPie.Functions;
internal static class DefaultFunctionsRegistrator
{
    public static void Register(FunctionsCollection defaultFunctions)
    {
        defaultFunctions.Register("$now", (string? format) => DateTime.Now.ToString(format));
        defaultFunctions.Register("$guid", Guid.NewGuid);
    }
}
