using TeaPie.Testing;
using static Xunit.Assert;

namespace TeaPie.Tests.Testing;

public class TestSchedulerShould
{
    [Fact]
    public void ScheduleTestCorrectly()
    {
        var scheduler = new TestScheduler();
        var test = new Test(
            string.Empty,
            async () => await Task.CompletedTask,
            new TestResult.NotRun() { TestName = string.Empty });

        scheduler.Schedule(test);

        True(scheduler.HasScheduledTest());

        var returned = scheduler.Dequeue();

        False(scheduler.HasScheduledTest());
        Same(test, returned);
    }
}
