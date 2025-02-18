
namespace TeaPie.Http.Headers;

internal abstract class ContentHeaderHandler : IHeaderHandler
{
    public abstract string HeaderName { get; }

    public bool CanResolve(string name, HttpRequestMessage responseMessage)
        => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase) && responseMessage.Content is not null;

    public bool CanResolve(string name, HttpResponseMessage requestMessage)
        => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase) && requestMessage.Content is not null;

    public abstract string GetHeader(HttpRequestMessage responseMessage);

    public abstract string GetHeader(HttpResponseMessage requestMessage);

    public abstract void SetHeader(string value, HttpRequestMessage requestMessage);
}
