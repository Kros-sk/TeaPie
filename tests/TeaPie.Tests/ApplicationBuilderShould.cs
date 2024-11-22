using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace TeaPie.Tests;

public class ApplicationBuilderShould
{
    [Fact]
    public void BuildMethodShouldReturnInstanceOfApplication()
    {
        var builder = ApplicationBuilder.Create();
        var app = builder.Build();

        app.Should().NotBeNull();
    }

    [Fact]
    public void ChainingOfMethodsShouldNotCauseAnyProblem()
    {
        var builder = ApplicationBuilder.Create();
        var app = builder
            .WithPath("path/to/collection")
            .WithTemporaryPath("path/to/custom/temp/folder")
            .AddLogging(LogLevel.Debug, "path/to/logFile")
            .Build();

        app.Should().NotBeNull();
    }

    [Fact]
    public async Task MultipleInvocationsOfSameMethodShouldNotCauseAnyProblem()
    {
        var builder = ApplicationBuilder.Create();
        var app = builder
            .WithPath("path/to/collection")
            .WithTemporaryPath("path/to/custom/temp/folder")
            .WithPath("i/have/changed/my/mind")
            .WithPath("or/lets/have/it/as/it/was")
            .AddLogging(LogLevel.Debug, "path/to/logFile")
            .AddLogging(LogLevel.Information, "path/to/logFile")
            .AddLogging(LogLevel.Warning)
            .Build();

        await app.Run();

        app.Should().NotBeNull();
    }
}
