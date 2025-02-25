namespace TeaPie.Http.Auth;

internal class AuthHttpMessageHandler(IAuthProviderAccessor accessor) : DelegatingHandler
{
    private readonly IAuthProviderAccessor _authProviderAccessor = accessor;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await GetAuthProvider().Authenticate(request, cancellationToken);
        return await base.SendAsync(request, cancellationToken);
    }

    private IAuthProvider GetAuthProvider()
        => _authProviderAccessor.CurrentProvider
            ?? throw new InvalidOperationException("Unable to authenticate with 'null' authentication provider.");
}
