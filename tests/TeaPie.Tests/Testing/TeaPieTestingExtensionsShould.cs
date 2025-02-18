using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Http.Retrying;
using TeaPie.Pipelines;
using TeaPie.Reporting;
using TeaPie.TestCases;
using TeaPie.Testing;
using TeaPie.Variables;
using static Xunit.Assert;
using File = TeaPie.StructureExploration.File;
using Folder = TeaPie.StructureExploration.Folder;
using TestCase = TeaPie.StructureExploration.TestCase;

namespace TeaPie.Tests.Testing;

[Collection(nameof(NonParallelCollection))]
public class TeaPieTestingExtensionsShould
{
    [Fact]
    public void ExecuteTestCorrectly()
    {
        var tester = PrepareTester();
        var teaPie = PrepareTeaPieInstance(tester);
        var executed = false;

        teaPie.Test("", () => executed = true);

        True(executed);
    }

    [Fact]
    public void SkipTestCorrectly()
    {
        var tester = PrepareTester();
        var teaPie = PrepareTeaPieInstance(tester);
        var executed = false;

        teaPie.Test("", () => executed = true, true);

        False(executed);
    }

    [Fact]
    public async Task ExecuteAsyncTestCorrectly()
    {
        var tester = PrepareTester();
        var teaPie = PrepareTeaPieInstance(tester);
        var executed = false;

        await teaPie.Test("", async () =>
        {
            executed = true;
            await Task.CompletedTask;
        });

        True(executed);
    }

    [Fact]
    public async Task SkipAsyncTestCorrectly()
    {
        var tester = PrepareTester();
        var teaPie = PrepareTeaPieInstance(tester);
        var executed = false;

        await teaPie.Test("", async () =>
        {
            executed = true;
            await Task.CompletedTask;
        }, true);

        False(executed);
    }

    private static Tester PrepareTester()
    {
        var accessor = new CurrentTestCaseExecutionContextAccessor()
        {
            Context = new TestCaseExecutionContext(
                new TestCase(
                    File.Create("path", new Folder(string.Empty, string.Empty, string.Empty, null))))
        };

        return new(
            accessor,
            Substitute.For<ITestResultsSummaryReporter>(),
            Substitute.For<ILogger<Tester>>());
    }

    private static TeaPie PrepareTeaPieInstance(ITester tester)
        => TeaPie.Create(
            Substitute.For<IVariables>(),
            Substitute.For<ILogger>(),
            tester,
            Substitute.For<ICurrentTestCaseExecutionContextAccessor>(),
             new ApplicationContextBuilder().Build(),
            Substitute.For<IPipeline>(),
            Substitute.For<ITestResultsSummaryReporter>(),
            Substitute.For<IRetryStrategyRegistry>());
}
