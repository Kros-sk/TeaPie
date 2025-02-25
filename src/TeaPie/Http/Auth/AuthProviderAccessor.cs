namespace TeaPie.Http.Auth;

internal interface IAuthProviderAccessor
{
    IAuthProvider? CurrentProvider { get; set; }
    IAuthProvider? DefaultProvider { get; set; }

    void SetCurrentProviderToDefault();
}

internal class AuthProviderAccessor : IAuthProviderAccessor
{
    public IAuthProvider? CurrentProvider { get; set; }
    public IAuthProvider? DefaultProvider { get; set; }

    public void SetCurrentProviderToDefault() => CurrentProvider = DefaultProvider;
}
