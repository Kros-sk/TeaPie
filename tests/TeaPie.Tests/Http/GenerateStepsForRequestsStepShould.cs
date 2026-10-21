using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Pipelines;
using TeaPie.Templating;
using TeaPie.TestCases;
using TeaPie.Variables;

namespace TeaPie.Tests.Http;

public class GenerateStepsForRequestsStepShould
{
    public static TheoryData<string[], (string, int)[]> RequestNames => new()
    {
        { ["CreatePartner1", "CreatePartner2"], [] },
        { [null!, null!], [] },
        { ["CreatePartner", "CreatePartner", "CreatePartner"], [("CreatePartner", 3)] },
        { ["CreatePartner", "CreatePartner", "CreateCompany", "CreateCompany"], [("CreatePartner", 2), ("CreateCompany", 2)] }
    };

    [Theory]
    [MemberData(nameof(RequestNames))]
    public void FindEachRequestNameUsedMoreThanOnce(string[] names, (string, int)[] expected)
    {
        var requests = names.Select(name =>
            $"### Request\n{(name is null ? string.Empty : $"# @name {name}\n")}POST https://example.com/x\n").ToArray();

        var duplicates = GenerateStepsForRequestsStep.FindDuplicateRequestNames(requests);

        duplicates.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task RegisterEachLoopGeneratedRequestUnderItsRenderedName()
    {
        var (context, appContext, insertedSteps, _) = await ExpandAndGenerateSteps("CreatePartner{{ forloop.index }}");

        insertedSteps.Should().HaveCount(6);
        await insertedSteps[0].Execute(appContext);
        await insertedSteps[3].Execute(appContext);
        context.Requests.Keys.Should().BeEquivalentTo(["CreatePartner1", "CreatePartner2"]);
    }

    [Theory]
    [InlineData("CreatePartner", true)]
    [InlineData("CreatePartner{{ forloop.index }}", false)]
    public async Task WarnOnlyWhenLoopGeneratedRequestsShareAName(string name, bool expectWarning)
    {
        var (_, _, insertedSteps, logger) = await ExpandAndGenerateSteps(name);

        insertedSteps.Should().HaveCount(6);
        CountDuplicateNameWarnings(logger).Should().Be(expectWarning ? 1 : 0);
    }

    [Fact]
    public async Task KeepRejectingDuplicateNamesInAFileWithoutTemplates()
    {
        const string content =
            "### First\n# @name Same\nPOST https://example.com/partners\n\n" +
            "### Second\n# @name Same\nPOST https://example.com/partners\n";

        var (context, appContext, insertedSteps, logger) = await ExpandAndGenerateStepsForContent(content);

        CountDuplicateNameWarnings(logger).Should().Be(0);
        context.AllowsDuplicateRequestNames.Should().BeFalse();
        await insertedSteps[0].Execute(appContext);
        var registerSecond = () => insertedSteps[3].Execute(appContext);
        await registerSecond.Should().ThrowAsync<ArgumentException>();
    }

    private static int CountDuplicateNameWarnings(ILogger<ApplicationContext> logger)
        => logger.ReceivedCalls().Count(call =>
            call.GetArguments()[0] is LogLevel.Warning
            && call.GetArguments()[2]?.ToString()?.Contains("requests named 'CreatePartner'") == true);

    private static Task<(TestCaseExecutionContext Context, ApplicationContext AppContext,
        IPipelineStep[] InsertedSteps, ILogger<ApplicationContext> Logger)> ExpandAndGenerateSteps(string requestName)
        => ExpandAndGenerateStepsForContent(
            "{% for partner in Temp.Partners %}### Create partner\n" +
            $"# @name {requestName}\n" +
            "POST https://example.com/partners\n" +
            "Content-Type: application/json\n\n" +
            "{ \"registrationId\": \"{{ partner.RegistrationId }}\" }\n" +
            "{% endfor %}");

    private static async Task<(TestCaseExecutionContext Context, ApplicationContext AppContext,
        IPipelineStep[] InsertedSteps, ILogger<ApplicationContext> Logger)> ExpandAndGenerateStepsForContent(
        string content)
    {
        var services = new ServiceCollection();
        var logger = Substitute.For<ILogger<ApplicationContext>>();
        services.AddSingleton(logger);
        services.AddTeaPie(false, () => { });
        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetRequiredService<IVariables>().SetVariable("Temp.Partners", new[]
        {
            new { RegistrationId = "01245" },
            new { RegistrationId = "012426" }
        });

        var context = RequestHelper.PrepareTestCaseContext(RequestsIndex.RequestWithFullStructure, false);
        context.RequestsFileContent = content;
        var accessor = new TestCaseExecutionContextAccessor { Context = context };
        var appContext = new ApplicationContextBuilder()
            .WithServiceProvider(serviceProvider)
            .WithPath(RequestsIndex.RootFolderFullPath)
            .Build();

        await new ExpandTemplatesStep(accessor, serviceProvider.GetRequiredService<ITemplateExpander>()).Execute(appContext);

        IPipelineStep[] insertedSteps = [];
        var pipeline = Substitute.For<IPipeline>();
        pipeline.When(p => p.InsertSteps(Arg.Any<IPipelineStep>(), Arg.Any<IPipelineStep[]>()))
            .Do(call => insertedSteps = call.ArgAt<IPipelineStep[]>(1));
        await new GenerateStepsForRequestsStep(accessor, pipeline).Execute(appContext);

        return (context, appContext, insertedSteps, logger);
    }
}
