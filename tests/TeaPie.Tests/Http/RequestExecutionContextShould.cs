using FluentAssertions;
using TeaPie.Http;
using TeaPie.StructureExploration;

namespace TeaPie.Tests.Http;

public class RequestExecutionContextShould
{
    private static InternalFile CreateFile()
    {
        var folder = new Folder("/path", "relative", "name");
        return new InternalFile("/path/test-req.http", "relative/test-req.http", folder);
    }

    [Fact]
    public void SetRequestFileFromConstructor()
    {
        var file = CreateFile();
        using var context = new RequestExecutionContext(file);

        context.RequestFile.Should().BeSameAs(file);
    }

    [Fact]
    public void HaveEmptyNameByDefault()
    {
        var file = CreateFile();
        using var context = new RequestExecutionContext(file);

        context.Name.Should().BeEmpty();
    }

    [Fact]
    public void HaveNullTestCaseExecutionContextByDefault()
    {
        var file = CreateFile();
        using var context = new RequestExecutionContext(file);

        context.TestCaseExecutionContext.Should().BeNull();
    }

    [Fact]
    public void AllowSettingProperties()
    {
        var file = CreateFile();
        using var context = new RequestExecutionContext(file);

        context.Name = "MyRequest";
        context.RawContent = "GET http://example.com";

        context.Name.Should().Be("MyRequest");
        context.RawContent.Should().Be("GET http://example.com");
    }

    [Fact]
    public void SetRawContentToNullOnDispose()
    {
        var file = CreateFile();
        var context = new RequestExecutionContext(file);
        context.RawContent = "some content";

        context.Dispose();

        context.RawContent.Should().BeNull();
    }
}
