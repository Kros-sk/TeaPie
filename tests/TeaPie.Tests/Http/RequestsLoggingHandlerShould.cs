using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using TeaPie.Http;
using TeaPie.Http.Auth;
using TeaPie.Logging;
using TeaPie.StructureExploration;
using static Xunit.Assert;

namespace TeaPie.Tests.Http;

public class RequestsLoggingHandlerShould
{
    private static readonly HttpRequestOptionsKey<RequestExecutionContext> _contextKey = new("__TeaPie_Context__");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LogRequestLogEntry(bool shouldFail)
    {
        var (handler, logger, innerHandler) = CreateHandler();
        var request = CreateRequest();
        var invoker = new HttpMessageInvoker(handler);

        if (shouldFail)
        {
            innerHandler.Exception = new HttpRequestException("Network error");
            await ThrowsAsync<HttpRequestException>(
                async () => await invoker.SendAsync(request, CancellationToken.None));
        }
        else
        {
            await invoker.SendAsync(request, CancellationToken.None);
        }

        logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => ValidateLogEntry(state, shouldFail)),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task NotLogWhenContextIsMissing()
    {
        var (handler, logger, _) = CreateHandler();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        var invoker = new HttpMessageInvoker(handler);
        await invoker.SendAsync(request, CancellationToken.None);

        logger.DidNotReceive().Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task LogMultipleConsecutiveRequests()
    {
        var (handler, logger, innerHandler) = CreateHandler();
        var invoker = new HttpMessageInvoker(handler);
        await invoker.SendAsync(CreateRequest(), CancellationToken.None);

        innerHandler.Exception = new HttpRequestException("Network error");
        await ThrowsAsync<HttpRequestException>(
            async () => await invoker.SendAsync(CreateRequest(), CancellationToken.None));

        innerHandler.Exception = null;
        await invoker.SendAsync(CreateRequest(), CancellationToken.None);

        logger.Received(3).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception?, string>>());

        logger.Received(2).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => ValidateLogEntry(state, false)),
            null,
            Arg.Any<Func<object, Exception?, string>>());

        logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => ValidateLogEntry(state, true)),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    private static bool ValidateLogEntry(object? state, bool shouldFail)
    {
        var entry = ExtractEntry(state);
        return entry != null
            && !string.IsNullOrEmpty(entry.RequestId)
            && entry.StartTime != default
            && entry.EndTime != null
            && entry.DurationMs >= 0
            && entry.Request.Method == "GET"
            && entry.Request.Uri == "https://example.com/"
            && entry.Request.FilePath == "test.http"
            && (shouldFail ? entry.Errors.Count > 0 : entry.Errors.Count == 0);
    }

    private static RequestLogFileEntry? ExtractEntry(object? state)
    {
        if (state is RequestLogFileEntry entry)
        {
            return entry;
        }

        if (state is IEnumerable<KeyValuePair<string, object?>> pairs)
        {
            return pairs.Select(kvp => kvp.Value).OfType<RequestLogFileEntry>().FirstOrDefault();
        }

        return null;
    }

    private static (RequestsLoggingHandler handler, ILogger logger, TestHttpMessageHandler innerHandler) CreateHandler()
    {
        var authAccessor = Substitute.For<IAuthProviderAccessor>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        var logger = Substitute.For<ILogger>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);

        var handler = new RequestsLoggingHandler(authAccessor, loggerFactory);
        var innerHandler = new TestHttpMessageHandler();
        handler.InnerHandler = innerHandler;

        return (handler, logger, innerHandler);
    }

    private static HttpRequestMessage CreateRequest()
    {
        var file = new InternalFile("test.http", "test.http", new Folder("", "", "", null));
        var context = new RequestExecutionContext(file);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        request.Options.Set(_contextKey, context);

        return request;
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        public HttpResponseMessage? Response { get; set; }
        public Exception? Exception { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (Exception != null)
            {
                throw Exception;
            }
            return Task.FromResult(Response ?? new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
