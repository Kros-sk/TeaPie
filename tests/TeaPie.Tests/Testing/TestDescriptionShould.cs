using FluentAssertions;
using TeaPie.Http;
using TeaPie.StructureExploration;
using TeaPie.Testing;

namespace TeaPie.Tests.Testing;

public class TestDescriptionShould
{
    [Fact]
    public void SetDirectiveFromConstructor()
    {
        var description = new TestDescription("TEST", new Dictionary<string, string>());

        description.Directive.Should().Be("TEST");
    }

    [Fact]
    public void SetParametersFromConstructor()
    {
        var parameters = new Dictionary<string, string> { { "key", "value" } };
        var description = new TestDescription("TEST", parameters);

        description.Parameters.Should().ContainKey("key");
    }

    [Fact]
    public void HaveNullRequestExecutionContextInitially()
    {
        var description = new TestDescription("TEST", new Dictionary<string, string>());

        description.RequestExecutionContext.Should().BeNull();
    }

    [Fact]
    public void SetRequestExecutionContextViaMethod()
    {
        var description = new TestDescription("TEST", new Dictionary<string, string>());
        var folder = new Folder("/path", "relative", "name");
        var file = new InternalFile("/path/test.http", "test.http", folder);
        var context = new RequestExecutionContext(file);

        description.SetRequestExecutionContext(context);

        description.RequestExecutionContext.Should().BeSameAs(context);
    }
}
