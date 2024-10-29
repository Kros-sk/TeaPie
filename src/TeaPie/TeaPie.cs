using Microsoft.Extensions.Logging;

namespace TeaPie;

public sealed class TeaPie
{
    // The way of TeaPie instance retrieval will be different in final version, this is just to make some functionality work
    public static TeaPie Create(ILogger logger) => Instance ??= new(logger);

    public static TeaPie? Instance { get; private set; }

    private TeaPie(ILogger logger)
    {
        Logger = logger;
    }

    public ILogger Logger { get; }
}
