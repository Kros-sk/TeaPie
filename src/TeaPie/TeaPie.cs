using Microsoft.Extensions.Logging;

namespace TeaPie;

internal class TeaPie
{
    private static TeaPie? _instance;

    //The way of TeaPie instance retrieval will be different in final version, this is just to make some functionality work
    internal static TeaPie Create(ILogger logger) => _instance ??= new(logger);

    internal static TeaPie? Instance => _instance;

    public TeaPie(ILogger logger)
    {
        Logger = logger;
    }

    public ILogger Logger { get; private set; }
}
