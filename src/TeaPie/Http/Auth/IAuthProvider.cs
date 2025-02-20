namespace TeaPie.Http.Auth;

public interface IAuthProvider
{
    Task Authenticate(HttpRequestMessage request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

public interface IAuthProvider<TOptions> : IAuthProvider where TOptions : IAuthProviderOptions
{
    void ConfigureOptions(TOptions options);
}
