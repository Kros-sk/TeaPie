﻿using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Data;
using TeaPie.Scripts;

namespace TeaPie.Tests.Scripts;

public class CompileScriptStepShould
{
    [Fact]
    public async Task CallCompileMethodOnCompilerDuringExecution()
    {
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.PlainScriptPath);
        var accessor = new ScriptExecutionContextAccessor() { Context = context };
        await ScriptHelper.PrepareScriptForCompilation(context);

        var compiler = Substitute.For<IScriptCompiler>();

        var appContext = new ApplicationContextBuilder().Build();

        var step = new CompileScriptStep(accessor, compiler);

        await step.Execute(appContext);

        compiler.Received(1).CompileScript(context.ProcessedContent!, context.Script.File.RelativePath);
    }

    [Fact]
    public async Task ThrowProperExceptionWhenCompilingScriptWithSyntaxError()
    {
        var logger = NullLogger.Instance;
        var context = ScriptHelper.GetScriptExecutionContext(ScriptIndex.ScriptWithSyntaxErrorPath);
        var accessor = new ScriptExecutionContextAccessor() { Context = context };
        await ScriptHelper.PrepareScriptForCompilation(context);

        var compiler = new ScriptCompiler(Substitute.For<ILogger<ScriptCompiler>>());

        var appContext = new ApplicationContextBuilder().Build();

        var step = new CompileScriptStep(accessor, compiler);

        await step.Invoking(async step => await step.Execute(appContext)).Should().ThrowAsync<SyntaxErrorException>();
    }
}
