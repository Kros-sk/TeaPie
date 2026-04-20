using TeaPie.Telemetry;

namespace TeaPie.Tests.Telemetry;

public class HttpRequestTelemetryShould
{
    [Theory]
    [InlineData(200, true)]
    [InlineData(201, true)]
    [InlineData(204, true)]
    [InlineData(299, true)]
    [InlineData(301, false)]
    [InlineData(400, false)]
    [InlineData(404, false)]
    [InlineData(500, false)]
    [InlineData(199, false)]
    public void CorrectlyDetermineIsSuccess(int statusCode, bool expectedIsSuccess)
    {
        var telemetry = new HttpRequestTelemetry { StatusCode = statusCode };

        Assert.Equal(expectedIsSuccess, telemetry.IsSuccess);
    }

    [Fact]
    public void HaveDefaultValuesSet()
    {
        var telemetry = new HttpRequestTelemetry();

        Assert.Equal(string.Empty, telemetry.Method);
        Assert.Equal(string.Empty, telemetry.Url);
        Assert.Equal(0, telemetry.StatusCode);
        Assert.Equal(0, telemetry.DurationMs);
        Assert.Equal(0, telemetry.RetryAttempts);
        Assert.False(telemetry.RetrySucceeded);
        Assert.Equal(string.Empty, telemetry.RequestName);
    }
}
