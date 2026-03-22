using TeaPie.Telemetry;

namespace TeaPie.Tests.Telemetry;

public class InMemoryTelemetryCollectorShould
{
    [Fact]
    public void NotBeEnabledByDefault()
    {
        var collector = new InMemoryTelemetryCollector();

        Assert.False(collector.IsEnabled);
    }

    [Fact]
    public void BeEnabledAfterCallingEnable()
    {
        var collector = new InMemoryTelemetryCollector();

        collector.Enable();

        Assert.True(collector.IsEnabled);
    }

    [Fact]
    public void UseDefaultOptionsWhenEnabledWithoutParameters()
    {
        var collector = new InMemoryTelemetryCollector();

        collector.Enable();

        Assert.True(collector.Options.EnableConsoleOutput);
        Assert.Null(collector.Options.CustomProvider);
    }

    [Fact]
    public void UseProvidedOptionsWhenEnabledWithOptions()
    {
        var collector = new InMemoryTelemetryCollector();
        var options = new TelemetryOptions { EnableConsoleOutput = false };

        collector.Enable(options);

        Assert.True(collector.IsEnabled);
        Assert.False(collector.Options.EnableConsoleOutput);
    }

    [Fact]
    public void NotRecordRequestsWhenDisabled()
    {
        var collector = new InMemoryTelemetryCollector();

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
        var collector = new InMemoryTelemetryCollector();
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
        var collector = new InMemoryTelemetryCollector();
        collector.Enable();

        var data1 = collector.GetTelemetryData();
        var data2 = collector.GetTelemetryData();

        Assert.Same(data1, data2);
    }

    [Fact]
    public async Task InvokeCustomProviderOnRequestRecorded()
    {
        var provider = new TestTelemetryProvider();
        var collector = new InMemoryTelemetryCollector();
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
        await Task.CompletedTask;
    }

    private class TestTelemetryProvider : ITelemetryProvider
    {
        public List<HttpRequestTelemetry> RecordedRequests { get; } = [];
        public TelemetryData? CompletedData { get; private set; }

        public Task OnRequestRecordedAsync(HttpRequestTelemetry telemetry)
        {
            RecordedRequests.Add(telemetry);
            return Task.CompletedTask;
        }

        public Task OnCollectionCompleteAsync(TelemetryData data)
        {
            CompletedData = data;
            return Task.CompletedTask;
        }
    }
}
