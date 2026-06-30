using FluentAssertions;
using Polly;
using TeaPie.Http.Retrying;

namespace TeaPie.Tests.Http.Retrying;

public class RetryingConstantsShould
{
    [Fact]
    public void DefaultName_IsRetry()
    {
        RetryingConstants.DefaultName.Should().Be("Retry");
    }

    [Fact]
    public void DefaultRetryCount_Is3()
    {
        RetryingConstants.DefaultRetryCount.Should().Be(3);
    }

    [Fact]
    public void MaxRetryCount_IsIntMaxValue()
    {
        RetryingConstants.MaxRetryCount.Should().Be(int.MaxValue);
    }

    [Fact]
    public void DefaultBackoffType_IsConstant()
    {
        RetryingConstants.DefaultBackoffType.Should().Be(DelayBackoffType.Constant);
    }

    [Fact]
    public void DefaultBaseDelay_Is2Seconds()
    {
        RetryingConstants.DefaultBaseDelay.Should().Be(TimeSpan.FromSeconds(2));
    }
}
