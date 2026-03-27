using Microsoft.Extensions.Logging;
using TeaPie.Pipelines;
using TeaPie.Scripts;
using Script = TeaPie.StructureExploration.Script;

namespace TeaPie.TestCases;

internal class InitializeTestCaseStep(ITestCaseExecutionContextAccessor accessor, IPipeline pipeline) : IPipelineStep
{
    private readonly IPipeline _pipeline = pipeline;
    private readonly ITestCaseExecutionContextAccessor _testCaseExecutionContextAccessor = accessor;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        ValidateContext(out var testCaseExecutionContext);

        context.CurrentTestCase = testCaseExecutionContext;

        if (testCaseExecutionContext.TestCase.IsFromTpFile)
        {
            PrepareTpTestCase(testCaseExecutionContext);
        }

        AddSteps(context, testCaseExecutionContext);

        LogTestCase(context, testCaseExecutionContext);

        await Task.CompletedTask;
    }

    private static void PrepareTpTestCase(TestCaseExecutionContext ctx)
    {
        var testCase = ctx.TestCase;
        var def = testCase.TpDefinition!;

        ctx.RequestsFileContent = def.HttpContent;

        var tpFilePath = testCase.RequestsFile.Path;
        var tpDir = Path.GetDirectoryName(tpFilePath) ?? string.Empty;
        var sanitizedName = SanitizeForFileName(testCase.Name);

        SetVirtualScript(def.InitContent, tpDir, sanitizedName, Constants.PreRequestSuffix,
            scripts => testCase.PreRequestScripts = scripts);
        SetVirtualScript(def.TestContent, tpDir, sanitizedName, Constants.PostResponseSuffix,
            scripts => testCase.PostResponseScripts = scripts);
    }

    private static void SetVirtualScript(
        string? content, string tpDir, string sanitizedName, string suffix, Action<Script[]> assign)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            var virtualPath = Path.Combine(tpDir, $"{sanitizedName}{suffix}{Constants.ScriptFileExtension}");
            var file = new StructureExploration.File(virtualPath, virtualPath);
            assign([new Script(file, content)]);
        }
    }

    private static string SanitizeForFileName(string name)
        => string.Join("_", name.Split(Path.GetInvalidFileNameChars()));

    private void AddSteps(ApplicationContext context, TestCaseExecutionContext testCaseExecutionContext)
    {
        List<IPipelineStep> newSteps = [];

        AddSteps(context, testCaseExecutionContext, newSteps);

        _pipeline.InsertSteps(this, [.. newSteps]);

        LogAdditionOfSteps(context, testCaseExecutionContext, newSteps);
    }

    private static void AddSteps(
        ApplicationContext context, TestCaseExecutionContext testCaseExecutionContext, List<IPipelineStep> newSteps)
    {
        AddStepsForPreRequestScripts(context, testCaseExecutionContext, newSteps);
        AddStepsForRegisterTestScripts(context, testCaseExecutionContext, newSteps);
        AddStepsForRequests(context, testCaseExecutionContext, newSteps);
        AddStepsForExecuteTestsScripts(context, testCaseExecutionContext, newSteps);
    }

    private static void AddStepsForPreRequestScripts(
        ApplicationContext context,
        TestCaseExecutionContext testCaseExecutionContext,
        List<IPipelineStep> newSteps)
        => AddStepsForScripts(
            context,
            testCaseExecutionContext.TestCase.PreRequestScripts,
            testCaseExecutionContext.RegisterPreRequestScript,
            newSteps);

    private static void AddStepsForRegisterTestScripts(
        ApplicationContext context,
        TestCaseExecutionContext testCaseExecutionContext,
        List<IPipelineStep> newSteps)
        => AddStepsForScripts(
            context,
            testCaseExecutionContext.TestCase.PostResponseScripts,
            testCaseExecutionContext.RegisterPostResponseScript,
            newSteps);

    private static void AddStepsForExecuteTestsScripts(
        ApplicationContext context,
        TestCaseExecutionContext testCaseExecutionContext,
        List<IPipelineStep> newSteps)
        => AddStepsForExecuteScript(
            context,
            testCaseExecutionContext,
            newSteps);

    private static void AddStepsForScripts(
        ApplicationContext context,
        IEnumerable<Script> scriptsCollection,
        Action<string, ScriptExecutionContext> addToCollection,
        List<IPipelineStep> newSteps)
    {
        foreach (var script in scriptsCollection)
        {
            addToCollection(script.File.Path, new(script));
            AddStepsForScript(context, script, newSteps);
        }
    }

    private static void AddStepsForScript(ApplicationContext context, Script script, List<IPipelineStep> newSteps)
    {
        var scriptExecutionContext = new ScriptExecutionContext(script)
        {
            RawContent = script.Content
        };

        var steps =
            ScriptStepsFactory.CreateStepsForScriptPreProcessAndExecution(context.ServiceProvider, scriptExecutionContext);

        newSteps.AddRange(steps);
    }

    private static void AddStepsForExecuteScript(
        ApplicationContext context,
        TestCaseExecutionContext caseExecutionContext,
        List<IPipelineStep> newSteps)
    {
        var steps =
            ScriptStepsFactory.CreateStepsForScriptExecution(context.ServiceProvider, caseExecutionContext);

        newSteps.AddRange(steps);
    }

    private static void AddStepsForRequests(
        ApplicationContext context,
        TestCaseExecutionContext testCaseExecutionContext,
        List<IPipelineStep> newSteps)
        => newSteps.AddRange(
            TestCaseStepsFactory.CreateStepsForRequestsWithinTestCase(context.ServiceProvider, testCaseExecutionContext));

    private static void LogAdditionOfSteps(
        ApplicationContext context, TestCaseExecutionContext testCaseExecutionContext, List<IPipelineStep> newSteps)
        => context.Logger.LogDebug("Multiple steps ({Count}) were scheduled in the pipeline for test-case execution " +
            "'{Name}'. ({Progress})",
                newSteps.Count,
                testCaseExecutionContext.TestCase.Name,
                $"{testCaseExecutionContext.Id}/{context.TestCases.Count}");

    private static void LogTestCase(ApplicationContext context, TestCaseExecutionContext testCaseExecutionContext)
        => context.Logger.LogInformation("Test case '{Name}' is going to be executed. ({Progress})",
            testCaseExecutionContext.TestCase.Name,
            $"{testCaseExecutionContext.Id}/{context.TestCases.Count}");

    private void ValidateContext(out TestCaseExecutionContext testCaseExecutionContext)
        => ExecutionContextValidator.Validate(
            _testCaseExecutionContextAccessor, out testCaseExecutionContext, "initialize test case");
}
