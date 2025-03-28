using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TeaPie.StructureExploration;
using TeaPie.TestCases;
using TeaPie.Tests.Http;
using TeaPie.Tests.Scripts;

namespace TeaPie.Tests.TestCases;

public class InitializeTestCaseStepShould
{
    private static readonly InternalFile _file = RequestHelper.GetFile(
            Path.Combine(ScriptIndex.RootSubFolderFullPath,
                $"virtualRequest{Constants.RequestSuffix}{Constants.RequestFileExtension}"));

    [Theory]
    [MemberData(nameof(GetVariousTestCases))]
    public async Task FulfillTestCaseContextProperlyDuringExecution(object testCaseAsObject)
    {
        if (testCaseAsObject is not TestCase testCase)
        {
            throw new NullReferenceException(nameof(testCase));
        }

        var executionContext = new TestCaseExecutionContext(testCase);
        var accessor = new TestCaseExecutionContextAccessor()
        {
            Context = executionContext
        };

        var services = new ServiceCollection();
        ConfigureServices(services);

        var provider = services.BuildServiceProvider();

        var appContext = new ApplicationContextBuilder()
            .WithPath(ScriptIndex.RootSubFolderFullPath)
            .WithServiceProvider(provider)
            .Build();

        var pipeline = new ApplicationPipeline();
        var step = new InitializeTestCaseStep(accessor, pipeline);

        pipeline.AddSteps(step);
        await step.Execute(appContext);

        executionContext.PreRequestScripts.Should().HaveCount(testCase.PreRequestScripts.Count());
        foreach (var preReq in testCase.PreRequestScripts)
        {
            executionContext.PreRequestScripts.TryGetValue(preReq.File.Path, out _).Should().BeTrue();
        }

        executionContext.PostResponseScripts.Should().HaveCount(testCase.PostResponseScripts.Count());
        foreach (var preReq in testCase.PostResponseScripts)
        {
            executionContext.PostResponseScripts.TryGetValue(preReq.File.Path, out _).Should().BeTrue();
        }
    }

    public static IEnumerable<object[]> GetVariousTestCases()
    {
        yield return [ new TestCase(_file)
        {
            PreRequestScripts = [],
            PostResponseScripts = []
        } ];

        yield return [ new TestCase(_file)
        {
            PreRequestScripts = [],
            PostResponseScripts = [GetScript(ScriptIndex.ScriptWithOneLoadDirectivePath)]
        } ];

        yield return [ new TestCase(_file)
        {
            PreRequestScripts = [GetScript(ScriptIndex.EmptyScriptPath), GetScript(ScriptIndex.ScriptManipulatingWithVariables)],
            PostResponseScripts = []
        } ];

        yield return [ new TestCase(_file)
        {
            PreRequestScripts = [GetScript(ScriptIndex.EmptyScriptPath), GetScript(ScriptIndex.ScriptManipulatingWithVariables)],
            PostResponseScripts = [GetScript(ScriptIndex.ScriptWithOneLoadDirectivePath), GetScript(ScriptIndex.EmptyScriptPath)]
        } ];
    }

    private static void ConfigureServices(ServiceCollection services)
        => services.AddTeaPie(true, () => { });

    private static Script GetScript(string path)
    {
        var folder = new Folder(
            ScriptIndex.RootSubFolderFullPath,
            ScriptIndex.RootSubFolderRelativePath,
            ScriptIndex.RootFolderName,
            null);

        var file = InternalFile.Create(path, folder);

        return new Script(file);
    }
}
