using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TeaPie.Pipelines;
using TeaPie.Scripts;
using TeaPie.TestCases;
using TeaPie.Testing;
using TeaPie.Variables;

namespace TeaPie.Tests.Scripts;

[Collection(nameof(NonParallelCollection))]
public class ExecuteScriptStepShould
{
    [Fact]
    public async void ExecuteScriptWithNuGetPackageWithoutAnyProblem()
    {
        var logger = NullLogger.Instance;
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithOneNuGetDirectivePath);
        var accessor = new ScriptExecutionContextAccessor() { Context = context };
        await ScriptHelper.PrepareScriptForExecution(context);

        var step = new ExecuteScriptStep(accessor);
        var appContext = new ApplicationContextBuilder()
            .WithLogger(logger)
            .Build();

        await step.Execute(appContext);
    }

    [Fact]
    public async void AccessTeaPieLoggerDuringScriptExectutionWithoutAnyProblem()
    {
        var logger = Substitute.For<ILogger>();
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptAccessingTeaPieLogger);
        var accessor = new ScriptExecutionContextAccessor() { Context = context };

        var step = new ExecuteScriptStep(accessor);
        var appContext = new ApplicationContextBuilder().Build();

        TeaPie.Create(
            Substitute.For<IVariables>(),
            logger,
            Substitute.For<ITester>(),
            Substitute.For<ICurrentTestCaseExecutionContextAccessor>(),
            appContext,
            Substitute.For<IPipeline>());

        await ScriptHelper.PrepareScriptForExecution(context);

        await step.Execute(appContext);

        logger.Received(1).LogInformation("It is possible to access TeaPie instance!");
    }

    [Fact]
    public async void BeAbleToManipulateWithVariablesDuringScriptExecution()
    {
        var logger = Substitute.For<ILogger>();
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptManipulatingWithVariables);
        var accessor = new ScriptExecutionContextAccessor() { Context = context };
        var variables = Substitute.For<IVariables>();
        variables.ContainsVariable("VariableToRemove").Returns(true);

        var step = new ExecuteScriptStep(accessor);
        var appContext = new ApplicationContextBuilder().Build();

        TeaPie.Create(
            variables,
            logger,
            Substitute.For<ITester>(),
            Substitute.For<ICurrentTestCaseExecutionContextAccessor>(),
            appContext,
            Substitute.For<IPipeline>());

        await ScriptHelper.PrepareScriptForExecution(context);

        await step.Execute(appContext);

        variables.Received(1).SetVariable("VariableToRemove", "anyValue");
        variables.Received(1).GetVariable<string>("VariableToRemove");
        variables.Received(1).ContainsVariable("VariableToRemove");
        variables.Received(1).RemoveVariable("VariableToRemove");
        variables.Received(1).SetVariable("VariableWithDeleteTag", "anyValue", "delete");
        variables.Received(1).RemoveVariablesWithTag("delete");
    }
}
