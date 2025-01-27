using System.Diagnostics.CodeAnalysis;

namespace TeaPie.Environments;

internal interface IEnvironmentsRegistry
{
    void RegisterEnvironment(Environment environment);

    bool TryGetEnvironment(string name, [NotNullWhen(true)] out Environment? environment);
}

internal class EnvironmentsRegistry : IEnvironmentsRegistry
{
    private readonly Dictionary<string, Environment> _environments = [];

    public bool TryGetEnvironment(string name, [NotNullWhen(true)] out Environment? environment)
        => _environments.TryGetValue(name, out environment);

    public void RegisterEnvironment(Environment environment)
        => _environments.Add(environment.Name, environment);
}
