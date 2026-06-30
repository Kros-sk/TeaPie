using Microsoft.Extensions.Logging;
using NSubstitute;
using TeaPie.Telemetry;

namespace TeaPie.Tests.Telemetry;

public class InMemoryTelemetryCollectorShould
{
    private static InMemoryTelemetryCollector CreateCollector()
        => new(Substitute.For<ILogger<InMemoryTelemetryCollector>>());

    [Fact]
    public void NotBeEnabledByDefault()
    {
        var collector = CreateCollector();

        Assert.False(collector.IsEnabled);
    }

    [Fact]
    public void BeEnabledAfterCallingEnable()
    {
        var collector = CreateCollector();

        collector.Enable();

        Assert.True(collector.IsEnabled);
    }

    [Fact]
    public void UseDefaultOptionsWhenEnabledWithoutParameters()
    {
        var collector = CreateCollector();

        collector.Enable();

        Assert.True(collector.Options.EnableConsoleOutput);
        Assert.Null(collector.Options.CustomProvider);
    }

    [Fact]
    public void UseProvidedOptionsWhenEnabledWithOptions()
    {
        var collector = CreateCollector();
        var options = new TelemetryOptions { EnableConsoleOutput = false };

        collector.Enable(options);

        Assert.True(collector.IsEnabled);
        Assert.False(collector.Options.EnableConsoleOutput);
    }

    [Fact]
    public void NotRecordRequestsWhenDisabled()
    {
        var collector = CreateCollector();

        collector.RecordRequest(new HttpRequestTelemetry
        {
            Method = "GET",
            Url = "https://api.example.com",
            StatusCode = 200,
            DurationMs = 100
        });

        Assert.Equal(0, collector.GetTelemetryData().TotalRequests);
    }

    [Fact]
    public void RecordRequestsWhenEnabled()
    {
        var collector = CreateCollector();
        collector.Enable();

        collector.RecordRequest(new HttpRequestTelemetry
        {
            Method = "GET",
            Url = "https://api.example.com",
            StatusCode = 200,
            DurationMs = 100
        });

        Assert.Equal(1, collector.GetTelemetryData().TotalRequests);
    }

    [Fact]
    public void ReturnSameTelemetryDataInstanceAcrossMultipleCalls()
    {
        var collector = CreateCollector();
        collector.Enable();

        var data1 = collector.GetTelemetryData();
        var data2 = collector.GetTelemetryData();

        Assert.Same(data1, data2);
    }

    [Fact]
    public void InvokeCustomProviderOnRequestRecorded()
    {
        var provider = new TestTelemetryProvider();
        var collector = CreateCollector();
        collector.Enable(new TelemetryOptions { CustomProvider = provider });

        var telemetry = new HttpRequestTelemetry
        {
            Method = "GET",
            Url = "https://api.example.com",
            StatusCode = 200,
            DurationMs = 100
        };

        collector.RecordRequest(telemetry);

        Assert.Single(provider.RecordedRequests);
        Assert.Same(telemetry, provider.RecordedRequests[0]);
    }

    [Fact]
    public void NotThrowWhenCustomProviderFails()
    {
        var provider = new FailingTelemetryProvider();
        var collector = CreateCollector();
        collector.Enable(new TelemetryOptions { CustomProvider = provider });

        var exception = Record.Exception(() => collector.RecordRequest(new HttpRequestTelemetry
        {
            Method = "GET",
            Url = "https://api.example.com",
            StatusCode = 200,
            DurationMs = 100
        }));

        Assert.Null(exception);
        Assert.Equal(1, collector.GetTelemetryData().TotalRequests);
    }

    private class TestTelemetryProvider : ITelemetryProvider
    {
        public List<HttpRequestTelemetry> RecordedRequests { get; } = [];

        public Task OnRequestRecordedAsync(HttpRequestTelemetry telemetry)
        {
            RecordedRequests.Add(telemetry);
            return Task.CompletedTask;
        }

        public Task OnCollectionCompleteAsync(TelemetryData data) => Task.CompletedTask;
    }

    private class FailingTelemetryProvider : ITelemetryProvider
    {
        public Task OnRequestRecordedAsync(HttpRequestTelemetry telemetry)
            => throw new InvalidOperationException("Provider failure");

        public Task OnCollectionCompleteAsync(TelemetryData data) => Task.CompletedTask;
    }
}
