using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Scripts;
using TeaPie.Tests.ScriptHandling;

namespace TeaPie.Tests.Pipelines.Scripts;
public class ExecuteScriptStepShould
{
    [Fact]
    public async void ScriptShouldAccessTeaPieInstanceWithoutAnyProblem()
    {
        var logger = NullLogger.Instance;
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptAccessingTeaPieInstance);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        TeaPie.Create(logger);
        await ScriptHelper.PrepareScriptForExecution(context);

        var step = new ExecuteScriptStep(accessor);
        var appContext = new ApplicationContext(string.Empty, logger, Substitute.For<IServiceProvider>());

        await step.Execute(appContext);
    }

    [Fact]
    public async void ScriptWithNuGetPackageShouldExecuteWithoutAnyProblem()
    {
        var logger = NullLogger.Instance;
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithOneNuGetDirectivePath);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        await ScriptHelper.PrepareScriptForExecution(context);

        var step = new ExecuteScriptStep(accessor);
        var appContext = new ApplicationContext(string.Empty, logger, Substitute.For<IServiceProvider>());

        await step.Execute(appContext);
    }
}
