using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Templating;
using TeaPie.Tests.Http;
using TeaPie.TestCases;

namespace TeaPie.Tests.Templating;

public class ExpandTemplatesStepShould
{
    [Fact]
    public async Task RewriteRequestsFileContentUsingTheTemplateExpander()
    {
        var context = RequestHelper.PrepareTestCaseContext(RequestsIndex.RequestWithFullStructure, false);
        context.RequestsFileContent = "{% for x in Xs %}BODY{% endfor %}";

        var expander = Substitute.For<ITemplateExpander>();
        expander.Expand("{% for x in Xs %}BODY{% endfor %}", context.TestCase.RequestsFile.RelativePath)
            .Returns("EXPANDED");

        var accessor = new TestCaseExecutionContextAccessor { Context = context };
        var step = new ExpandTemplatesStep(accessor, expander);

        var appContext = new ApplicationContextBuilder().WithPath(RequestsIndex.RootFolderFullPath).Build();
        await step.Execute(appContext);

        context.RequestsFileContent.Should().Be("EXPANDED");
    }

    [Fact]
    public async Task LogFullyExpandedContentAtTraceLevel()
    {
        var context = RequestHelper.PrepareTestCaseContext(RequestsIndex.RequestWithFullStructure, false);
        context.RequestsFileContent = "{% for x in Xs %}BODY{% endfor %}";

        var expander = Substitute.For<ITemplateExpander>();
        expander.Expand("{% for x in Xs %}BODY{% endfor %}", context.TestCase.RequestsFile.RelativePath)
            .Returns("EXPANDED");

        var accessor = new TestCaseExecutionContextAccessor { Context = context };
        var step = new ExpandTemplatesStep(accessor, expander);

        var logger = Substitute.For<ILogger<ApplicationContext>>();
        var appContext = new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .WithLogger(logger)
            .Build();

        await step.Execute(appContext);

        logger.Received(1).Log(
            LogLevel.Trace,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => ContainsExpandedContent(state, "EXPANDED")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [InlineData("GET https://example.test")]
    [InlineData("POST https://example.test\n\n{\"fmt\":\"{%Y-%m-%d}\"}")]
    public async Task NotLogExpansionDetailsForAFileWithoutATemplateTag(string content)
    {
        var context = RequestHelper.PrepareTestCaseContext(RequestsIndex.RequestWithFullStructure, false);
        context.RequestsFileContent = content;

        var expander = Substitute.For<ITemplateExpander>();
        expander.Expand(content, Arg.Any<string>()).Returns(content);

        var accessor = new TestCaseExecutionContextAccessor { Context = context };
        var step = new ExpandTemplatesStep(accessor, expander);

        var logger = Substitute.For<ILogger<ApplicationContext>>();
        var appContext = new ApplicationContextBuilder()
            .WithPath(RequestsIndex.RootFolderFullPath)
            .WithLogger(logger)
            .Build();

        await step.Execute(appContext);

        context.RequestsFileContent.Should().BeSameAs(content);
        logger.DidNotReceive().Log(
            Arg.Is<LogLevel>(level => level == LogLevel.Debug || level == LogLevel.Trace),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    private static bool ContainsExpandedContent(object? state, string expectedContent)
        => state is IEnumerable<KeyValuePair<string, object?>> pairs
            && pairs.Any(kvp => kvp.Value as string == expectedContent);
}
