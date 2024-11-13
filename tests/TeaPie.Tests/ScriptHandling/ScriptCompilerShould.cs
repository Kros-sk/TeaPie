using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Data;
using TeaPie.ScriptHandling;

namespace TeaPie.Tests.ScriptHandling;

public class ScriptCompilerShould
{
    [Fact]
    public async void ScriptWithSyntaxErrorShouldThrowException()
    {
        var logger = NullLogger.Instance;
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithSyntaxErrorPath);
        await ScriptHelper.PrepareScriptForCompilation(context);

        var compiler = new ScriptCompiler(Substitute.For<ILogger<ScriptCompiler>>());

        compiler.Invoking(c => c.CompileScript(context.ProcessedContent!)).Should().Throw<SyntaxErrorException>();
    }

    [Fact]
    public async void PlainScriptShouldBeCompiledWithoutAnyProblem()
    {
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.PlainScriptPath);
        await ScriptHelper.PrepareScriptForCompilation(context);

        var compiler = new ScriptCompiler(Substitute.For<ILogger<ScriptCompiler>>());

        var compiledScript = compiler.CompileScript(context.ProcessedContent!);
        compiledScript.Should().NotBe(null);
    }

    [Fact]
    public async void ScriptWithNuGetPackageShouldBeCompiledWithoutAnyProblem()
    {
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithOneNuGetDirectivePath);
        await ScriptHelper.PrepareScriptForCompilation(context);

        var compiler = new ScriptCompiler(Substitute.For<ILogger<ScriptCompiler>>());

        var compiledScript = compiler.CompileScript(context.ProcessedContent!);
        compiledScript.Should().NotBe(null);
    }
}
