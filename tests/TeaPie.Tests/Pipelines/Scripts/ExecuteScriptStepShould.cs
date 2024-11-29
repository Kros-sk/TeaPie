using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Scripts;
using TeaPie.Tests.Scripts;
using TeaPie.Variables;

namespace TeaPie.Tests.Pipelines.Scripts;

[Collection(nameof(NonParallelCollection))]
public class ExecuteScriptStepShould
{
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
            Substitute.For<IServiceProvider>());

        await step.Execute(appContext);
    }

    [Fact]
    public async void AccessTeaPieLoggerDuringScriptExectutionWithoutAnyProblem()
    {
        var logger = Substitute.For<ILogger>();
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptAccessingTeaPieLogger);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        TeaPie.Create(Substitute.For<IVariables>(), logger);
        await ScriptHelper.PrepareScriptForExecution(context);

        var step = new ExecuteScriptStep(accessor);
        var appContext = new ApplicationContext(
            string.Empty,
            Substitute.For<ILogger<ApplicationContext>>(),
            Substitute.For<IServiceProvider>());

        await step.Execute(appContext);

        logger.Received(1).LogInformation("It is possible to access TeaPie instance!");
    }

    [Fact]
    public async void BeAbleToManipulateWithVariablesDuringScriptExecution()
    {
        var logger = Substitute.For<ILogger>();
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptManipulatingWithVariables);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        var variables = Substitute.For<IVariables>();
        variables.ContainsVariable("VariableToRemove").Returns(true);

        TeaPie.Create(variables, logger);

        await ScriptHelper.PrepareScriptForExecution(context);

        var step = new ExecuteScriptStep(accessor);
        var appContext = new ApplicationContext(
            string.Empty,
            Substitute.For<ILogger<ApplicationContext>>(),
            Substitute.For<IServiceProvider>());

        await step.Execute(appContext);

        variables.Received(1).SetVariable("VariableToRemove", "anyValue");
        variables.Received(1).GetVariable<string>("VariableToRemove");
        variables.Received(1).ContainsVariable("VariableToRemove");
        variables.Received(1).RemoveVariable("VariableToRemove");
        variables.Received(1).SetVariable("VariableWithDeleteTag", "anyValue", "delete");
        variables.Received(1).RemoveVariablesWithTag("delete");
    }
}
