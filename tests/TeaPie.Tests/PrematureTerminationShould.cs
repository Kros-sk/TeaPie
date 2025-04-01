using Microsoft.Extensions.DependencyInjection;
using TeaPie.Pipelines;
using static Xunit.Assert;

namespace TeaPie.Tests;

[Collection(nameof(NonParallelCollection))]
public class PrematureTerminationShould
{
    [Fact]
    public async Task BeTriggeredByApplicationError()
    {
        var services = new ServiceCollection();
        services.AddTeaPie(true, () => { });
        var provider = services.BuildServiceProvider();

        var pipeline = provider.GetRequiredService<IPipeline>();
        var step = new InlineStep((_, _) => throw new Exception("Application Error!"));
        pipeline.AddSteps(step);

        var exitCode = await pipeline.Run(new ApplicationContextBuilder().Build(), CancellationToken.None);

        Equal(1, exitCode);
    }

    [Fact]
    public async Task BeTriggeredByUserAction()
    {
        const int exitCode = 12;
        var services = new ServiceCollection();
        services.AddTeaPie(true, () => { });
        var provider = services.BuildServiceProvider();
        var appContext = new ApplicationContextBuilder().Build();

        var teaPie = new TeaPieBuilder().WithApplicationContext(appContext).Build();

        var pipeline = provider.GetRequiredService<IPipeline>();
        var step = new InlineStep(async (_, _) =>
        {
            teaPie.Exit(exitCode);
            await Task.CompletedTask;
        });
        pipeline.AddSteps(step);

        var realeExitCode = await pipeline.Run(appContext, CancellationToken.None);

        Equal(exitCode, realeExitCode);
    }
}
