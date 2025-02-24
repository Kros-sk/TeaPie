namespace TeaPie.Http.Auth;

internal interface IAuthProviderRegistry : IRegistry<IAuthProvider>;

internal class AuthProviderRegistry : IAuthProviderRegistry
{
    private readonly Dictionary<string, IAuthProvider> _registry =
        new(StringComparer.OrdinalIgnoreCase) { { AuthConstants.NoAuthKey, new NoAuthProvider() } };

    public void Register(string name, IAuthProvider retryStrategy)
        => _registry.Add(name, retryStrategy);

    public IAuthProvider Get(string name) => _registry[name];

    public bool IsRegistered(string name) => _registry.ContainsKey(name);
}
