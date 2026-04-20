using NSubstitute;
using TeaPie.Telemetry;

namespace TeaPie.Tests.Telemetry;

public class ReportTelemetrySummaryStepShould
{
    [Fact]
    public async Task NotReportWhenTelemetryIsDisabled()
    {
        var collector = Substitute.For<ITelemetryCollector>();
        collector.IsEnabled.Returns(false);

        var step = new ReportTelemetrySummaryStep(collector);
        var context = new ApplicationContextBuilder()
            .WithPath(System.IO.Path.GetTempPath())
            .Build();

        await step.Execute(context);

        collector.DidNotReceive().GetTelemetryData();
    }

    [Fact]
    public async Task RetrieveTelemetryDataWhenEnabled()
    {
        var data = new TelemetryData();
        var options = new TelemetryOptions { EnableConsoleOutput = false };
        var collector = Substitute.For<ITelemetryCollector>();
        collector.IsEnabled.Returns(true);
        collector.Options.Returns(options);
        collector.GetTelemetryData().Returns(data);

        var step = new ReportTelemetrySummaryStep(collector);
        var context = new ApplicationContextBuilder()
            .WithPath(System.IO.Path.GetTempPath())
            .Build();

        await step.Execute(context);

        collector.Received(1).GetTelemetryData();
    }

    [Fact]
    public async Task InvokeCustomProviderOnCollectionComplete()
    {
        var provider = Substitute.For<ITelemetryProvider>();
        var data = new TelemetryData();
        var options = new TelemetryOptions
        {
            EnableConsoleOutput = false,
            CustomProvider = provider
        };

        var collector = Substitute.For<ITelemetryCollector>();
        collector.IsEnabled.Returns(true);
        collector.Options.Returns(options);
        collector.GetTelemetryData().Returns(data);

        var step = new ReportTelemetrySummaryStep(collector);
        var context = new ApplicationContextBuilder()
            .WithPath(System.IO.Path.GetTempPath())
            .Build();

        await step.Execute(context);

        await provider.Received(1).OnCollectionCompleteAsync(data);
    }
}
