﻿using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Data;
using TeaPie.Scripts;

namespace TeaPie.Tests.Scripts;

public class ScriptCompilerShould
{
    [Fact]
    public async Task ThrowProperExceptionWhenCompilingScriptWithSyntaxError()
    {
        var logger = NullLogger.Instance;
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithSyntaxErrorPath);
        await ScriptHelper.PrepareScriptForCompilation(context);

        var compiler = new ScriptCompiler(Substitute.For<ILogger<ScriptCompiler>>());

        compiler.Invoking(c => c.CompileScript(context.ProcessedContent!, context.Script.File.RelativePath))
            .Should().Throw<SyntaxErrorException>();
    }

    [Fact]
    public async Task CompilePlainScriptWithoutAnyProblem()
    {
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.PlainScriptPath);
        await ScriptHelper.PrepareScriptForCompilation(context);

        var compiler = new ScriptCompiler(Substitute.For<ILogger<ScriptCompiler>>());

        var compiledScript = compiler.CompileScript(context.ProcessedContent!, context.Script.File.RelativePath);
        compiledScript.Should().NotBe(null);
    }

    [Fact]
    public async Task CompileScriptWithNuGetPackageWithoutAnyProblem()
    {
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithOneNuGetDirectivePath);
        await ScriptHelper.PrepareScriptForCompilation(context);

        var compiler = new ScriptCompiler(Substitute.For<ILogger<ScriptCompiler>>());

        var compiledScript = compiler.CompileScript(context.ProcessedContent!, context.Script.File.RelativePath);
        compiledScript.Should().NotBe(null);
    }
}
