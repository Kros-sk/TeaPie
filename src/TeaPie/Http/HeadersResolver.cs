using System.Net.Http.Headers;

namespace TeaPie.Http;

internal interface IHeaderResolver
{
    bool CanResolve(string name);

    void ResolveHeader(
        string value,
        HttpRequestMessage requestExecutionContext);
}

internal interface IHeadersResolver
{
    void Resolve(HttpParsingContext parsingContext, HttpRequestMessage requestExecutionContext);
}

internal class HeadersResolver : IHeadersResolver
{
    private readonly IHeaderResolver[] _resolvers =
    {
        new ContentTypeHeaderResolver(),
        new ContentDispositionHeaderResolver(),
        new ContentEncodingHeaderResolver(),
        new ContentLanguageHeaderResolver(),
        new AuthorizationHeaderResolver(),
        new UserAgentHeaderResolver(),
        new DateHeaderResolver(),
        new ConnectionHeaderResolver(),
        new HostHeaderResolver()
    };

    public void Resolve(HttpParsingContext parsingContext, HttpRequestMessage requestExecutionContext)
    {
        ResolveNormalHeaders(parsingContext, requestExecutionContext);
        ResolveSpecialHeaders(parsingContext, requestExecutionContext);
    }

    private static void ResolveNormalHeaders(HttpParsingContext parsingContext, HttpRequestMessage requestExecutionContext)
    {
        foreach (var header in parsingContext.Headers)
        {
            if (!requestExecutionContext.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                throw new InvalidOperationException($"Unable to resolve header '{header.Key} : {header.Value}'");
            }
        }
    }

    private void ResolveSpecialHeaders(HttpParsingContext parsingContext, HttpRequestMessage requestExecutionContext)
    {
        bool resolved;
        foreach (var header in parsingContext.SpecialHeaders)
        {
            resolved = false;

            foreach (var resolver in _resolvers)
            {
                if (resolver.CanResolve(header.Key))
                {
                    resolver.ResolveHeader(header.Value, requestExecutionContext);
                    resolved = true;
                    break;
                }
            }

            if (!resolved)
            {
                throw new InvalidOperationException($"Unable to resolve header '{header.Key} : {header.Value}'");
            }
        }
    }

    private class ContentTypeHeaderResolver : IHeaderResolver
    {
        const string HeaderName = "Content-Type";

        public bool CanResolve(string name) => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

        public void ResolveHeader(string value, HttpRequestMessage requestExecutionContext)
        {
            if (requestExecutionContext.Content is not null)
            {
                requestExecutionContext.Content.Headers.ContentType = new MediaTypeHeaderValue(value);
            }
            else
            {
                throw new InvalidOperationException($"Unable to resolve header '{HeaderName}' when content is null.");
            }
        }
    }

    private class ContentDispositionHeaderResolver : IHeaderResolver
    {
        const string HeaderName = "Content-Disposition";

        public bool CanResolve(string name) => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

        public void ResolveHeader(string value, HttpRequestMessage requestExecutionContext)
        {
            if (requestExecutionContext.Content is null)
            {
                throw new InvalidOperationException($"Unable to resolve header '{HeaderName}' when content is null.");
            }
            else
            {
                requestExecutionContext.Content.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse(value);
            }
        }
    }

    private class ContentEncodingHeaderResolver : IHeaderResolver
    {
        const string HeaderName = "Content-Encoding";

        public bool CanResolve(string name) => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

        public void ResolveHeader(string value, HttpRequestMessage requestExecutionContext)
        {
            if (requestExecutionContext.Content is not null)
            {
                requestExecutionContext.Content.Headers.ContentEncoding.Add(value);
            }
            else
            {
                throw new InvalidOperationException($"Unable to resolve header '{HeaderName}' when content is null.");
            }
        }
    }

    private class ContentLanguageHeaderResolver : IHeaderResolver
    {
        const string HeaderName = "Content-Language";

        public bool CanResolve(string name) => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

        public void ResolveHeader(string value, HttpRequestMessage requestExecutionContext)
        {
            if (requestExecutionContext.Content is not null)
            {
                requestExecutionContext.Content.Headers.ContentLanguage.Add(value);
            }
            else
            {
                throw new InvalidOperationException($"Unable to resolve header '{HeaderName}' when content is null.");
            }
        }
    }

    private class AuthorizationHeaderResolver : IHeaderResolver
    {
        const string HeaderName = "Authorization";

        public bool CanResolve(string name) => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

        public void ResolveHeader(string value, HttpRequestMessage requestExecutionContext)
        {
            var parts = value.Split(' ', 2);
            if (parts.Length != 2)
            {
                throw new InvalidOperationException($"Invalid format for '{HeaderName}' header. Expected: 'Scheme Value'.");
            }
            requestExecutionContext.Headers.Authorization = new AuthenticationHeaderValue(parts[0], parts[1]);
        }
    }

    private class UserAgentHeaderResolver : IHeaderResolver
    {
        const string HeaderName = "User-Agent";

        public bool CanResolve(string name) => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

        public void ResolveHeader(string value, HttpRequestMessage requestExecutionContext)
        {
            requestExecutionContext.Headers.UserAgent.ParseAdd(value);
        }
    }

    private class DateHeaderResolver : IHeaderResolver
    {
        const string HeaderName = "Date";

        public bool CanResolve(string name) => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

        public void ResolveHeader(string value, HttpRequestMessage requestExecutionContext)
        {
            if (DateTimeOffset.TryParse(value, out var parsedDate))
            {
                requestExecutionContext.Headers.Date = parsedDate;
            }
            else
            {
                throw new InvalidOperationException($"Invalid format for '{HeaderName}' header value.");
            }
        }
    }

    private class ConnectionHeaderResolver : IHeaderResolver
    {
        const string HeaderName = "Connection";

        public bool CanResolve(string name) => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

        public void ResolveHeader(string value, HttpRequestMessage requestExecutionContext)
        {
            if (value.Equals("close", StringComparison.OrdinalIgnoreCase))
            {
                requestExecutionContext.Headers.ConnectionClose = true;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported '{HeaderName}' value: {value}");
            }
        }
    }

    private class HostHeaderResolver : IHeaderResolver
    {
        const string HeaderName = "Host";

        public bool CanResolve(string name) => name.Equals(HeaderName, StringComparison.OrdinalIgnoreCase);

        public void ResolveHeader(string value, HttpRequestMessage requestExecutionContext)
        {
            requestExecutionContext.Headers.Host = value;
        }
    }
}
