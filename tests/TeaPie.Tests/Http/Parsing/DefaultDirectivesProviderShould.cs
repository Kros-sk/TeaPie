using System.Net;
using TeaPie.Http.Parsing;
using TeaPie.Testing;
using Xunit.Sdk;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Parsing;

public class DefaultDirectivesProviderShould
{
    private static readonly TestDirective _hasBodyDirective = DefaultDirectivesProvider.GetDefaultTestDirectives()
        .Single(d => d.Name == TestDirectives.TestHasBodyDirectiveFullName);

    [Fact]
    public async Task PassHasBodyTestWhenResponseHasBody()
    {
        using var response = CreateResponse(HttpStatusCode.OK, "{\"id\":1}");

        await _hasBodyDirective.TestFunction(response, HasBodyParameters(true));
    }

    [Fact]
    public async Task KeepResponseBodyReadableAfterHasBodyTest()
    {
        const string body = "{\"id\":1}";
        using var response = CreateResponse(HttpStatusCode.OK, body);

        await _hasBodyDirective.TestFunction(response, HasBodyParameters(true));

        Equal(body, await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData(HttpStatusCode.NoContent, null)]
    [InlineData(HttpStatusCode.OK, "")]
    public async Task FailHasBodyTestWhenResponseHasNoBody(HttpStatusCode statusCode, string? body)
    {
        using var response = CreateResponse(statusCode, body);

        await ThrowsAnyAsync<XunitException>(() => _hasBodyDirective.TestFunction(response, HasBodyParameters(true)));
    }

    [Theory]
    [InlineData(HttpStatusCode.NoContent, null)]
    [InlineData(HttpStatusCode.OK, "")]
    public async Task PassNegatedHasBodyTestWhenResponseHasNoBody(HttpStatusCode statusCode, string? body)
    {
        using var response = CreateResponse(statusCode, body);

        await _hasBodyDirective.TestFunction(response, HasBodyParameters(false));
    }

    [Fact]
    public async Task FailNegatedHasBodyTestWhenResponseHasBody()
    {
        using var response = CreateResponse(HttpStatusCode.OK, "{\"id\":1}");

        await ThrowsAnyAsync<XunitException>(() => _hasBodyDirective.TestFunction(response, HasBodyParameters(false)));
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string? body)
    {
        var response = new HttpResponseMessage(statusCode);
        if (body is not null)
        {
            response.Content = new StringContent(body);
        }

        return response;
    }

    private static Dictionary<string, string> HasBodyParameters(bool shouldHaveBody)
        => new() { [TestDirectives.TestHasBodyDirectiveParameterName] = shouldHaveBody.ToString() };
}
