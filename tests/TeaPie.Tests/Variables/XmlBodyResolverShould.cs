using FluentAssertions;
using TeaPie.Variables;

namespace TeaPie.Tests.Variables;

public class XmlBodyResolverShould
{
    private readonly XmlBodyResolver _resolver = new();

    [Fact]
    public void CanResolveApplicationXml()
    {
        _resolver.CanResolve("application/xml").Should().BeTrue();
    }

    [Fact]
    public void CanResolveTextXml()
    {
        _resolver.CanResolve("text/xml").Should().BeTrue();
    }

    [Fact]
    public void CanResolveTextXmlCaseInsensitive()
    {
        _resolver.CanResolve("TEXT/XML").Should().BeTrue();
    }

    [Fact]
    public void NotResolveApplicationJson()
    {
        _resolver.CanResolve("application/json").Should().BeFalse();
    }

    [Fact]
    public void ResolveValueForXPathQuery()
    {
        var body = "<root><name>John</name></root>";
        _resolver.Resolve(body, "/root/name").Should().Be("John");
    }

    [Fact]
    public void ReturnDefaultWhenNodeNotFound()
    {
        var body = "<root><name>John</name></root>";
        _resolver.Resolve(body, "/root/age", "N/A").Should().Be("N/A");
    }
}
