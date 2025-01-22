using FluentAssertions;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using TeaPie.Http;
using static Xunit.Assert;

namespace TeaPie.Tests.Http;

public class HttpMessagesExtensionsShould
{
    private static HttpRequestMessage CreateRequestWithContent(string content)
        => new() { Content = new StringContent(content) };

    private static HttpResponseMessage CreateResponseWithContent(string content)
        => new() { Content = new StringContent(content) };

    #region Synchronous get body methods
    [Fact]
    public void ReturnStringContentWhenGetBodyCalledOnRequestWithContent()
    {
        var request = CreateRequestWithContent("Request Body Content");

        var result = request.GetBody();

        result.Should().Be("Request Body Content");
    }

    [Fact]
    public void ReturnEmptyStringWhenGetBodyCalledOnRequestWithoutContent()
    {
        var request = new HttpRequestMessage();

        var result = request.GetBody();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ReturnStringContentWhenGetBodyCalledOnResponseWithContent()
    {
        var response = CreateResponseWithContent("Response Body Content");

        var result = response.GetBody();

        result.Should().Be("Response Body Content");
    }

    [Fact]
    public void ReturnEmptyObjectWhenGetBodyCalledOnResponseWithoutContent()
    {
        var response = new HttpResponseMessage();

        var result = response.GetBody();

        result.Should().BeEmpty();
    }

    [Fact]
    public void ReturnClassWithContentWhenGetTypedBodyCalledOnRequestWithContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var request = CreateRequestWithContent(body);

        var result = request.GetBody<Dummy>();

        CompareDummies(result, data);
        CompareDummies(result.Parent, data.Parent);
    }

    [Fact]
    public void ThrowExceptionWhenGetTypedBodyCalledOnRequestWithoutContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var request = CreateRequestWithContent(string.Empty);

        Throws<JsonException>(request.GetBody<Dummy>);
    }

    [Fact]
    public void ReturnClassWithContentWhenGetTypedBodyCalledOnResponseWithContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var response = CreateResponseWithContent(body);

        var result = response.GetBody<Dummy>();

        CompareDummies(result, data);
        CompareDummies(result.Parent, data.Parent);
    }

    [Fact]
    public void ThrowExceptionWhenGetTypedBodyCalledOnResponseWithoutContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var response = CreateResponseWithContent(string.Empty);

        Throws<JsonException>(response.GetBody<Dummy>);
    }

    [Fact]
    public void ReturnDynamicCaseInsensitiveExpandoObjectWhenGetBodyAsExpandoCalledOnRequestWithContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var request = CreateRequestWithContent(body);

        dynamic result = request.GetBodyAsExpando();

        CompareDynamic(result, data);
        CompareDynamic(result.Parent, data.Parent);
    }

    [Fact]
    public void ReturnEmptyDynamicCaseInsensitiveExpandoObjectWhenGetBodyAsExpandoCalledOnRequestWithoutContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var request = CreateRequestWithContent(string.Empty);

        dynamic result = request.GetBodyAsExpando();

        Throws<RuntimeBinderException>(() => result.Id);
    }

    [Fact]
    public void ReturnDynamicCaseInsensitiveExpandoObjectWhenGetBodyAsExpandoCalledOnResponseWithContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var response = CreateResponseWithContent(body);

        dynamic result = response.GetBodyAsExpando();

        CompareDynamic(result, data);
        CompareDynamic(result.Parent, data.Parent);
    }

    [Fact]
    public void ReturnEmptyDynamicCaseInsensitiveExpandoObjectWhenGetBodyAsExpandoCalledOnResponseWithoutContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var response = CreateResponseWithContent(string.Empty);

        dynamic result = response.GetBodyAsExpando();

        Throws<RuntimeBinderException>(() => result.Id);
    }
    #endregion

    #region Asynchronous get body methods
    [Fact]
    public async Task ReturnStringContentWhenGetBodyAsyncCalledOnRequestWithContent()
    {
        var request = CreateRequestWithContent("Async Request Body Content");

        var result = await request.GetBodyAsync();

        result.Should().Be("Async Request Body Content");
    }

    [Fact]
    public async Task ReturnEmptyStringWhenGetBodyAsyncCalledOnRequestWithoutContent()
    {
        var request = new HttpRequestMessage();

        var result = await request.GetBodyAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ReturnStringContentWhenGetBodyAsyncCalledOnResponseWithContent()
    {
        var response = CreateResponseWithContent("Async Response Body Content");

        var result = await response.GetBodyAsync();

        result.Should().Be("Async Response Body Content");
    }

    [Fact]
    public async Task ReturnEmptyStringWhenGetBodyAsyncCalledOnResponseWithoutContent()
    {
        var response = new HttpResponseMessage();

        var result = await response.GetBodyAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ReturnClassWithContentWhenGetTypedBodyAsyncCalledOnRequestWithContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var request = CreateRequestWithContent(body);

        var result = await request.GetBodyAsync<Dummy>();

        CompareDummies(result, data);
        CompareDummies(result.Parent, data.Parent);
    }

    [Fact]
    public async Task ThrowExceptionWhenGetTypedBodyAsyncCalledOnRequestWithoutContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var request = CreateRequestWithContent(string.Empty);

        await ThrowsAsync<JsonException>(request.GetBodyAsync<Dummy>);
    }

    [Fact]
    public async Task ReturnClassWithContentWhenGetTypedBodyAsyncCalledOnResponseWithContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var response = CreateResponseWithContent(body);

        var result = await response.GetBodyAsync<Dummy>();

        CompareDummies(result, data);
        CompareDummies(result.Parent, data.Parent);
    }

    [Fact]
    public async Task ThrowExceptionWhenGetTypedBodyAsyncCalledOnResponseWithoutContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var response = CreateResponseWithContent(string.Empty);

        await ThrowsAsync<JsonException>(response.GetBodyAsync<Dummy>);
    }

    [Fact]
    public async Task ReturnDynamicCaseInsensitiveExpandoObjectWhenGetBodyAsExpandoAsyncCalledOnRequestWithContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var request = CreateRequestWithContent(body);

        dynamic result = await request.GetBodyAsExpandoAsync();

        CompareDynamic(result, data);
        CompareDynamic(result.Parent, data.Parent);
    }

    [Fact]
    public async Task ReturnEmptyDynamicCaseInsensitiveExpandoObjectWhenGetBodyAsExpandoAsyncCalledOnRequestWithoutContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var request = CreateRequestWithContent(string.Empty);

        dynamic result = await request.GetBodyAsExpandoAsync();

        Throws<RuntimeBinderException>(() => result.Id);
    }

    [Fact]
    public async Task ReturnDynamicCaseInsensitiveExpandoObjectWhenGetBodyAsExpandoAsyncCalledOnResponseWithContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var response = CreateResponseWithContent(body);

        dynamic result = await response.GetBodyAsExpandoAsync();

        CompareDynamic(result, data);
        CompareDynamic(result.Parent, data.Parent);
    }

    [Fact]
    public async Task ReturnEmptyDynamicCaseInsensitiveExpandoObjectWhenGetBodyAsExpandoAsyncCalledOnResponseWithoutContent()
    {
        var data = GetDummy();
        var body = JsonSerializer.Serialize(data);
        var response = CreateResponseWithContent(string.Empty);

        dynamic result = await response.GetBodyAsExpandoAsync();

        Throws<RuntimeBinderException>(() => result.Id);
    }
    #endregion

    [Fact]
    public void ReturnCorrectStatusCodeWhenStatusCodeCalledOnResponse()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        var statusCode = response.StatusCode();

        statusCode.Should().Be(200);
    }

    private static void CompareDummies([NotNull] Dummy? result, Dummy? data)
    {
        NotNull(result);
        NotNull(data);
        Equal(result.Id, data.Id);
        Equal(result.Name, data.Name);
        Equal(result.IsRegistered, data.IsRegistered);
        Equal(result.Averages.Length, data.Averages.Length);
        for (var i = 0; i < result.Averages.Length; i++)
        {
            Equal(result.Averages[i], data.Averages[i]);
        }
    }

    private static void CompareDynamic(dynamic real, Dummy expected)
    {
        NotNull(real);
        NotNull(expected);
        Equal(real.Id, expected.Id);
        Equal(real.Name, expected.Name);
        Equal(real.IsRegistered, expected.IsRegistered);
        Equal(real.Averages.Count, expected.Averages.Length);
        foreach (var (Expected, Real) in expected.Averages.Zip(((List<object>)real.Averages).Select(x => ((JValue)x).Value)))
        {
            Equal(Expected, Real);
        }
    }

    private static Dummy GetDummy()
        => new()
        {
            Id = 1,
            Averages = [2.2, 5.5, 8.9],
            Name = "Bar",
            IsRegistered = true,
            Parent = new Dummy()
            {
                Id = 2,
                Averages = [],
                Name = "Foo",
                IsRegistered = true
            }
        };

    private class Dummy
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsRegistered { get; set; }
        public double[] Averages { get; set; } = [];
        public Dummy? Parent { get; set; }
    }
}
