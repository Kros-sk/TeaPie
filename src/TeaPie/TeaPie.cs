using Microsoft.Extensions.Logging;

namespace TeaPie;

public sealed class TeaPie
{
    internal static TeaPie Create(ILogger logger) => Instance ??= new(logger);

    public static TeaPie? Instance { get; private set; }

    private TeaPie(ILogger logger)
    {
        Logger = logger;
    }

    public ILogger Logger { get; }
}
