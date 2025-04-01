using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TeaPie.Environments;
using TeaPie.Scripts;
using TeaPie.Variables;

namespace TeaPie.Tests.Scripts;

[Collection(nameof(NonParallelCollection))]
public class ExecuteScriptStepShould
{
    [Fact]
    public async Task ExecuteScriptWithNuGetPackageWithoutAnyProblem()
    {
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithOneNuGetDirectivePath);
        var accessor = new ScriptExecutionContextAccessor() { Context = context };
        await ScriptHelper.PrepareScriptForExecution(context);

        var step = new ExecuteScriptStep(accessor);
        var appContext = new ApplicationContextBuilder().Build();

        await step.Execute(appContext);
    }

    [Fact]
    public async Task AccessTeaPieLoggerDuringScriptExectutionWithoutAnyProblem()
    {
        const string envName = "CustomEnvironment";
        var logger = NullLogger.Instance;
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptAccessingTeaPieLogger);
        var accessor = new ScriptExecutionContextAccessor() { Context = context };
        var services = new ServiceCollection();
        services.AddTeaPie(true, () => { });
        var serviceProvider = services.BuildServiceProvider();

        var environment = new global::TeaPie.Environments.Environment(envName, []);
        var registry = serviceProvider.GetRequiredService<IEnvironmentsRegistry>();
        registry.Register(envName, environment);

        var step = new ExecuteScriptStep(accessor);
        var appContext = new ApplicationContextBuilder().WithServiceProvider(serviceProvider).Build();

        PrepareTeaPieInstance(logger, appContext);

        await ScriptHelper.PrepareScriptForExecution(context);

        await step.Execute(appContext);
        Assert.Equal(envName, TeaPie.Instance!.ApplicationContext.EnvironmentName);
    }

    [Fact]
    public async Task BeAbleToManipulateWithVariablesDuringScriptExecution()
    {
        var logger = Substitute.For<ILogger>();
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptManipulatingWithVariables);
        var accessor = new ScriptExecutionContextAccessor() { Context = context };
        var variables = Substitute.For<IVariables>();
        variables.ContainsVariable("VariableToRemove").Returns(true);

        var step = new ExecuteScriptStep(accessor);
        var appContext = new ApplicationContextBuilder().Build();

        PrepareTeaPieInstance(logger, appContext, variables);

        await ScriptHelper.PrepareScriptForExecution(context);

        await step.Execute(appContext);

        variables.Received(1).SetVariable("VariableToRemove", "anyValue");
        variables.Received(1).GetVariable<string>("VariableToRemove");
        variables.Received(1).ContainsVariable("VariableToRemove");
        variables.Received(1).RemoveVariable("VariableToRemove");
        variables.Received(1).SetVariable("VariableWithDeleteTag", "anyValue", "delete");
        variables.Received(1).RemoveVariablesWithTag("delete");
    }

    private static void PrepareTeaPieInstance(ILogger logger, ApplicationContext appContext, IVariables? variables = null)
        => new TeaPieBuilder()
            .WithApplicationContext(appContext)
            .WithService(variables ?? Substitute.For<IVariables>())
            .WithService(logger)
            .Build();
}
