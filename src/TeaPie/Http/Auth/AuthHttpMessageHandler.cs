namespace TeaPie.Http.Auth;

internal class AuthHttpMessageHandler(IDefaultAuthProviderAccessor accessor) : DelegatingHandler
{
    private readonly IDefaultAuthProviderAccessor _authProviderAccessor = accessor;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await GetAuthProvider().Authenticate(request, cancellationToken);
        return await base.SendAsync(request, cancellationToken);
    }

    private IAuthProvider GetAuthProvider()
        => _authProviderAccessor.DefaultProvider
            ?? throw new InvalidOperationException("Unable to work with 'null' default authentication provider.");
}
