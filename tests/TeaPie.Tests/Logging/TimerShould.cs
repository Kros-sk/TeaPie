using FluentAssertions;
using Timer = TeaPie.Logging.Timer;

namespace TeaPie.Tests.Logging;

public class TimerShould
{
    [Fact]
    public async Task InvokeLogCallbackAndReturnResultForAsyncExecute()
    {
        long loggedMs = -1;

        var result = await Timer.Execute(
            async () => { await Task.Delay(10); return 42; },
            ms => loggedMs = ms);

        result.Should().Be(42);
        loggedMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void InvokeLogCallbackAndReturnResultForSyncExecute()
    {
        long loggedMs = -1;

        var result = Timer.Execute(
            () => 42,
            ms => loggedMs = ms);

        result.Should().Be(42);
        loggedMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void InvokeActionAndLogCallbackForVoidExecute()
    {
        long loggedMs = -1;
        var actionInvoked = false;

        Timer.Execute(
            () => actionInvoked = true,
            ms => loggedMs = ms);

        actionInvoked.Should().BeTrue();
        loggedMs.Should().BeGreaterThanOrEqualTo(0);
    }
}
