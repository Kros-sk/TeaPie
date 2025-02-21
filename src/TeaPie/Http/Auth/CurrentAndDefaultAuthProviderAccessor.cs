namespace TeaPie.Http.Auth;

internal interface ICurrentAndDefaultAuthProviderAccessor
{
    IAuthProvider? CurrentProvider { get; set; }
    IAuthProvider? DefaultProvider { get; set; }

    void SetCurrentProviderToDefault();
}

internal class CurrentAndDefaultAuthProviderAccessor : ICurrentAndDefaultAuthProviderAccessor
{
    public IAuthProvider? CurrentProvider { get; set; }
    public IAuthProvider? DefaultProvider { get; set; }

    public void SetCurrentProviderToDefault() => CurrentProvider = DefaultProvider;
}
