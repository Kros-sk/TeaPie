using FluentAssertions;
using TeaPie.Http.Parsing;
using TeaPie.Testing;

namespace TeaPie.Tests.Http.Parsing;

public class HttpParsingContextShould
{
    private static HttpParsingContext CreateContext()
    {
        using var msg = new HttpRequestMessage();
        return new HttpParsingContext(msg.Headers);
    }

    [Fact]
    public void HaveEmptyInitialProperties()
    {
        var context = CreateContext();

        context.RequestName.Should().BeEmpty();
        context.RequestUri.Should().BeEmpty();
        context.RetryStrategyName.Should().BeEmpty();
        context.AuthProviderName.Should().BeEmpty();
    }

    [Fact]
    public void AddHeaderToHeadersDictionary()
    {
        var context = CreateContext();

        context.AddHeader("Content-Type", "application/json");

        context.Headers.Should().ContainKey("Content-Type");
        context.Headers["Content-Type"].Should().Be("application/json");
    }

    [Fact]
    public void AddMultipleHeaders()
    {
        var context = CreateContext();

        context.AddHeader("Accept", "text/plain");
        context.AddHeader("Authorization", "Bearer token");

        context.Headers.Should().ContainKey("Accept");
        context.Headers.Should().ContainKey("Authorization");
    }

    [Fact]
    public void RegisterTestAddsToTestsList()
    {
        var context = CreateContext();
        var testDesc = new TestDescription("TEST", new Dictionary<string, string>());

        context.RegiterTest(testDesc);

        context.Tests.Should().ContainSingle();
    }

    [Fact]
    public void HaveNonNullBodyBuilder()
    {
        var context = CreateContext();

        context.BodyBuilder.Should().NotBeNull();
    }

    [Fact]
    public void HaveDefaultFalseForBoolProperties()
    {
        var context = CreateContext();

        context.IsBody.Should().BeFalse();
        context.IsMethodAndUriResolved.Should().BeFalse();
    }
}
