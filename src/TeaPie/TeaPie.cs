using Microsoft.Extensions.Logging;

namespace TeaPie;

public sealed class TeaPie
{
    // TODO: Change the way of instance retrieval.
    public static TeaPie Create(ILogger logger) => Instance ??= new(logger);

    public static TeaPie? Instance { get; private set; }

    private TeaPie(ILogger logger)
    {
        Logger = logger;
    }

    public ILogger Logger { get; }
}
