using FluentAssertions;
using TeaPie.Variables;

namespace TeaPie.Tests.Variables;

public class JsonBodyResolverShould
{
    private readonly JsonBodyResolver _resolver = new();

    [Fact]
    public void CanResolveApplicationJson()
    {
        _resolver.CanResolve("application/json").Should().BeTrue();
    }

    [Fact]
    public void CanResolveApplicationJsonCaseInsensitive()
    {
        _resolver.CanResolve("APPLICATION/JSON").Should().BeTrue();
    }

    [Fact]
    public void NotResolveTextXml()
    {
        _resolver.CanResolve("text/xml").Should().BeFalse();
    }

    [Fact]
    public void ResolveSimpleProperty()
    {
        var body = """{"name":"John"}""";
        _resolver.Resolve(body, "name").Should().Be("John");
    }

    [Fact]
    public void ReturnDefaultWhenTokenNotFound()
    {
        var body = """{"name":"John"}""";
        _resolver.Resolve(body, "age", "N/A").Should().Be("N/A");
    }

    [Fact]
    public void ResolveNestedValueWithJPath()
    {
        var body = """{"user":{"name":"John"}}""";
        _resolver.Resolve(body, "user.name").Should().Be("John");
    }
}
