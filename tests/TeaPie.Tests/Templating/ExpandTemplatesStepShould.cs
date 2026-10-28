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
            .Returns(new TemplateExpansionResult("EXPANDED", TemplatedRequestNames.None));

        var accessor = new TestCaseExecutionContextAccessor { Context = context };
        var step = new ExpandTemplatesStep(accessor, expander, Substitute.For<IExpandedContentDumper>());

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
            .Returns(new TemplateExpansionResult("EXPANDED", TemplatedRequestNames.None));

        var accessor = new TestCaseExecutionContextAccessor { Context = context };
        var step = new ExpandTemplatesStep(accessor, expander, Substitute.For<IExpandedContentDumper>());

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
        expander.Expand(content, Arg.Any<string>())
            .Returns(new TemplateExpansionResult(content, TemplatedRequestNames.None));

        var accessor = new TestCaseExecutionContextAccessor { Context = context };
        var step = new ExpandTemplatesStep(accessor, expander, Substitute.For<IExpandedContentDumper>());

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

    [Theory]
    [InlineData("{% for x in Xs %}BODY{% endfor %}", true, true)]
    [InlineData("{% for x in Xs %}BODY{% endfor %}", false, false)]
    [InlineData("GET https://example.test/plain", true, false)]
    public async Task DumpTheExpandedContentOnlyForATemplatedFileWhenRequested(
        string content, bool dumpRequested, bool expectedDump)
    {
        var context = RequestHelper.PrepareTestCaseContext(RequestsIndex.RequestWithFullStructure, false);
        context.RequestsFileContent = content;

        var expander = Substitute.For<ITemplateExpander>();
        expander.Expand(content, Arg.Any<string>())
            .Returns(new TemplateExpansionResult("EXPANDED", TemplatedRequestNames.None));

        var dumper = Substitute.For<IExpandedContentDumper>();
        var accessor = new TestCaseExecutionContextAccessor { Context = context };
        var step = new ExpandTemplatesStep(accessor, expander, dumper);

        var appContextBuilder = new ApplicationContextBuilder().WithPath(RequestsIndex.RootFolderFullPath);
        if (dumpRequested)
        {
            appContextBuilder.WithExpandedContentDump();
        }

        var appContext = appContextBuilder.Build();

        await step.Execute(appContext);

        dumper.ReceivedCalls().Should().HaveCount(expectedDump ? 1 : 0);
        if (expectedDump)
        {
            dumper.Received(1).Dump(appContext, context.TestCase, "EXPANDED");
        }
    }

    private static bool ContainsExpandedContent(object? state, string expectedContent)
        => state is IEnumerable<KeyValuePair<string, object?>> pairs
            && pairs.Any(kvp => kvp.Value as string == expectedContent);
}
