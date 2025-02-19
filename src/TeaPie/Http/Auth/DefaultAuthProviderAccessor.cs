namespace TeaPie.Http.Auth;

internal interface IDefaultAuthProviderAccessor
{
    IAuthProvider? DefaultProvider { get; set; }
}

internal class DefaultAuthProviderAccessor : IDefaultAuthProviderAccessor
{
    public IAuthProvider? DefaultProvider { get; set; }
}
