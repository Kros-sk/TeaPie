namespace TeaPie.Environments;

internal interface IEnvironmentsRegistry : IRegistry<Environment>;

internal class EnvironmentsRegistry : IEnvironmentsRegistry
{
    private readonly Dictionary<string, Environment> _environments = [];

    public Environment Get(string name)
        => _environments[name];

    public void Register(string name, Environment environment)
        => _environments.Add(environment.Name, environment);

    public bool IsRegistered(string name)
        => _environments.ContainsKey(name);
}
