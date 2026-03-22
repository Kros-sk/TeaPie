using TeaPie.Telemetry;

namespace TeaPie.Tests.Telemetry;

public class TelemetryDataShould
{
    [Fact]
    public void ReturnZeroValuesWhenNoRequestsRecorded()
    {
        var data = new TelemetryData();

        Assert.Equal(0, data.TotalRequests);
        Assert.Equal(0, data.SuccessfulRequests);
        Assert.Equal(0, data.SuccessRate);
        Assert.Equal(0, data.AverageDurationMs);
        Assert.Equal(0, data.MinDurationMs);
        Assert.Equal(0, data.MaxDurationMs);
        Assert.Equal(0, data.TotalRetryAttempts);
        Assert.Equal(0, data.RequestsWithRetries);
        Assert.Equal(0, data.RetrySuccessRate);
        Assert.Equal(0, data.RetryPercentage);
        Assert.Equal(0, data.TotalDurationSeconds);
        Assert.Equal(0, data.Throughput);
        Assert.Null(data.FastestRequest);
        Assert.Null(data.SlowestRequest);
        Assert.Null(data.MostRetriedRequest);
    }

    [Fact]
    public void CorrectlyCalculateStatisticsForSingleRequest()
    {
        var data = new TelemetryData();

        data.RecordRequest(new HttpRequestTelemetry
        {
            Method = "GET",
            Url = "https://api.example.com/users",
            StatusCode = 200,
            DurationMs = 150,
            RetryAttempts = 0,
            RequestName = "GetUsers"
        });

        Assert.Equal(1, data.TotalRequests);
        Assert.Equal(1, data.SuccessfulRequests);
        Assert.Equal(100, data.SuccessRate);
        Assert.Equal(150, data.AverageDurationMs);
        Assert.Equal(150, data.MinDurationMs);
        Assert.Equal(150, data.MaxDurationMs);
        Assert.Equal(0, data.TotalRetryAttempts);
        Assert.Equal(0, data.RequestsWithRetries);
    }

    [Fact]
    public void CorrectlyCalculateStatisticsForMultipleRequests()
    {
        var data = new TelemetryData();

        data.RecordRequest(new HttpRequestTelemetry
        {
            Method = "GET",
            Url = "https://api.example.com/users",
            StatusCode = 200,
            DurationMs = 100
        });

        data.RecordRequest(new HttpRequestTelemetry
        {
            Method = "POST",
            Url = "https://api.example.com/users",
            StatusCode = 201,
            DurationMs = 200
        });

        data.RecordRequest(new HttpRequestTelemetry
        {
            Method = "GET",
            Url = "https://api.example.com/users/1",
            StatusCode = 404,
            DurationMs = 50
        });

        data.RecordRequest(new HttpRequestTelemetry
        {
            Method = "DELETE",
            Url = "https://api.example.com/users/1",
            StatusCode = 500,
            DurationMs = 300
        });

        Assert.Equal(4, data.TotalRequests);
        Assert.Equal(2, data.SuccessfulRequests);
        Assert.Equal(50, data.SuccessRate);
        Assert.Equal(162.5, data.AverageDurationMs);
        Assert.Equal(50, data.MinDurationMs);
        Assert.Equal(300, data.MaxDurationMs);
    }

    [Fact]
    public void CorrectlyIdentifyFastestAndSlowestRequests()
    {
        var data = new TelemetryData();

        var fast = new HttpRequestTelemetry
        {
            Method = "GET",
            Url = "https://api.example.com/fast",
            StatusCode = 200,
            DurationMs = 45
        };

        var slow = new HttpRequestTelemetry
        {
            Method = "POST",
            Url = "https://api.example.com/slow",
            StatusCode = 200,
            DurationMs = 1200
        };

        data.RecordRequest(fast);
        data.RecordRequest(new HttpRequestTelemetry { Method = "GET", Url = "https://api.example.com/mid", StatusCode = 200, DurationMs = 234 });
        data.RecordRequest(slow);

        Assert.Equal(fast, data.FastestRequest);
        Assert.Equal(slow, data.SlowestRequest);
    }

    [Fact]
    public void CorrectlyTrackRetryStatistics()
    {
        var data = new TelemetryData();

        data.RecordRequest(new HttpRequestTelemetry
        {
            Method = "GET",
            Url = "https://api.example.com/stable",
            StatusCode = 200,
            DurationMs = 100,
            RetryAttempts = 0
        });

        data.RecordRequest(new HttpRequestTelemetry
        {
            Method = "POST",
            Url = "https://api.example.com/flaky",
            StatusCode = 200,
            DurationMs = 500,
            RetryAttempts = 3,
            RetrySucceeded = true
        });

        data.RecordRequest(new HttpRequestTelemetry
        {
            Method = "POST",
            Url = "https://api.example.com/broken",
            StatusCode = 500,
            DurationMs = 800,
            RetryAttempts = 4,
            RetrySucceeded = false
        });

        Assert.Equal(7, data.TotalRetryAttempts);
        Assert.Equal(2, data.RequestsWithRetries);
        Assert.Equal(50, data.RetrySuccessRate);
        Assert.True(Math.Abs(66.67 - data.RetryPercentage) < 0.1);
    }

    [Fact]
    public void CorrectlyIdentifyMostRetriedRequest()
    {
        var data = new TelemetryData();

        data.RecordRequest(new HttpRequestTelemetry
        {
            Method = "GET",
            Url = "https://api.example.com/a",
            StatusCode = 200,
            DurationMs = 100,
            RetryAttempts = 2
        });

        var mostRetried = new HttpRequestTelemetry
        {
            Method = "POST",
            Url = "https://api.example.com/b",
            StatusCode = 200,
            DurationMs = 500,
            RetryAttempts = 5
        };

        data.RecordRequest(mostRetried);

        data.RecordRequest(new HttpRequestTelemetry
        {
            Method = "PUT",
            Url = "https://api.example.com/c",
            StatusCode = 200,
            DurationMs = 200,
            RetryAttempts = 1
        });

        Assert.Equal(mostRetried, data.MostRetriedRequest);
    }

    [Fact]
    public void CalculateThroughputCorrectly()
    {
        var data = new TelemetryData();

        // 10 requests each taking 100ms = 1000ms total = 1 second
        for (int i = 0; i < 10; i++)
        {
            data.RecordRequest(new HttpRequestTelemetry
            {
                Method = "GET",
                Url = $"https://api.example.com/item/{i}",
                StatusCode = 200,
                DurationMs = 100
            });
        }

        Assert.Equal(1.0, data.TotalDurationSeconds);
        Assert.Equal(10.0, data.Throughput);
    }
}
