namespace TeaPie;

internal class TeaPie
{
    private static TeaPie? _instance;

    //The way of TeaPie instance retrieval will be different in final version, this is just to make some functionality work
    internal static TeaPie GetInstance() => _instance ??= new();
}
