namespace TeaPie.Http.Auth;

internal interface IAuthProviderRegistry
{
    void RegisterAuthProvider(string name, IAuthProvider retryStrategy);

    IAuthProvider GetAuthProvider(string name);

    bool IsAuthProviderRegistered(string name);
}

internal class AuthProviderRegistry : IAuthProviderRegistry
{
    private readonly Dictionary<string, IAuthProvider> _registry =
        new(StringComparer.OrdinalIgnoreCase) { { AuthConstants.NoAuthKey, new NoAuthProvider() } };

    public void RegisterAuthProvider(string name, IAuthProvider retryStrategy)
        => _registry.Add(name, retryStrategy);

    public IAuthProvider GetAuthProvider(string name) => _registry[name];

    public bool IsAuthProviderRegistered(string name) => _registry.ContainsKey(name);
}
