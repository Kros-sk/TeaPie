using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Data;
using TeaPie.Pipelines.Application;
using TeaPie.Pipelines.Scripts;
using TeaPie.Scripts;
using TeaPie.Tests.Scripts;

namespace TeaPie.Tests.Pipelines.Scripts;

public class CompileScriptStepShould
{
    [Fact]
    public async void CompilerShouldReceiveCallToCompileTheScript()
    {
        var logger = NullLogger.Instance;
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.PlainScriptPath);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        await ScriptHelper.PrepareScriptForCompilation(context);

        var compiler = Substitute.For<IScriptCompiler>();

        var appContext = new ApplicationContext(string.Empty, logger, Substitute.For<IServiceProvider>());
        var step = new CompileScriptStep(accessor, compiler);

        await step.Execute(appContext);

        compiler.Received(1).CompileScript(context.ProcessedContent!);
    }

    [Fact]
    public async void ScriptWithSyntaxErrorShouldThrowProperException()
    {
        var logger = NullLogger.Instance;
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithSyntaxErrorPath);
        var accessor = new ScriptExecutionContextAccessor() { ScriptExecutionContext = context };
        await ScriptHelper.PrepareScriptForCompilation(context);

        var compiler = new ScriptCompiler(Substitute.For<ILogger<ScriptCompiler>>());

        var appContext = new ApplicationContext(string.Empty, logger, Substitute.For<IServiceProvider>());
        var step = new CompileScriptStep(accessor, compiler);

        await step.Invoking(async step => await step.Execute(appContext)).Should().ThrowAsync<SyntaxErrorException>();
    }
}
