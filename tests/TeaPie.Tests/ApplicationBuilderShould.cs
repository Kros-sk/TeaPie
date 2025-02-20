using FluentAssertions;
using Microsoft.Extensions.Logging;
using TeaPie.Application;

namespace TeaPie.Tests;

[Collection(nameof(NonParallelCollection))]
public class ApplicationBuilderShould
{
    [Fact]
    public void ShouldReturnInstanceOfApplicationWhenCallingBuild()
    {
        var builder = ApplicationBuilder.Create();
        var app = builder.Build();

        app.Should().NotBeNull();
    }

    [Fact]
    public void EnableChainingOfMethodsWithoutAnyProblem()
    {
        var builder = ApplicationBuilder.Create();
        var app = builder
            .WithPath("path/to/collection")
            .WithTemporaryPath("path/to/custom/temp/folder")
            .WithLogging(LogLevel.Debug, "path/to/logFile", LogLevel.Debug)
            .Build();

        app.Should().NotBeNull();
    }

    [Fact]
    public void EnableMultipleInvocationsOfSameMethodWithoutAnyProblem()
    {
        var builder = ApplicationBuilder.Create();
        var app = builder
            .WithPath("path/to/collection")
            .WithTemporaryPath("path/to/custom/temp/folder")
            .WithPath("i/have/changed/my/mind")
            .WithPath("or/lets/have/it/as/it/was")
            .WithLogging(LogLevel.Debug, "path/to/logFile")
            .WithLogging(LogLevel.Information, "path/to/logFile")
            .WithLogging(LogLevel.Warning)
            .Build();

        app.Should().NotBeNull();
    }
}
