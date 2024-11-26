using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Scripts;
using TeaPie.Tests.Scripts;
using TeaPie.Variables;

namespace TeaPie.Tests.Pipelines.Scripts;

public class ExecuteScriptStepShould
{
    [Fact]
    public async void ScriptShouldAccessTeaPieInstanceWithoutAnyProblem()
    {
        var logger = Substitute.For<ILogger>();
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptAccessingTeaPieInstance);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        var variables = Substitute.For<IVariables>();
        TeaPie.Create(variables, logger);
        await ScriptHelper.PrepareScriptForExecution(context);

        var step = new ExecuteScriptStep(accessor);
        var appContext = new ApplicationContext(
            string.Empty,
            Substitute.For<ILogger<ApplicationContext>>(),
            Substitute.For<IServiceProvider>(),
            variables);

        await step.Execute(appContext);

        logger.Received(1).LogInformation("It is possible to access TeaPie instance!");
    }

    [Fact]
    public async void ScriptWithNuGetPackageShouldExecuteWithoutAnyProblem()
    {
        var logger = NullLogger.Instance;
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithOneNuGetDirectivePath);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        await ScriptHelper.PrepareScriptForExecution(context);

        var step = new ExecuteScriptStep(accessor);
        var appContext = new ApplicationContext(
            string.Empty,
            logger,
            Substitute.For<IServiceProvider>(),
            Substitute.For<IVariables>());

        await step.Execute(appContext);
    }
}
