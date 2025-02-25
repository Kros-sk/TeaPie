namespace TeaPie.Testing;

internal interface ITestScheduler
{
    void Schedule(Test test);

    bool HasScheduledTest();

    Test Dequeue();
}

internal class TestScheduler : ITestScheduler
{
    private readonly Queue<Test> _tests = [];

    public void Schedule(Test test) => _tests.Enqueue(test);

    public bool HasScheduledTest() => _tests.Count != 0;

    public Test Dequeue() => _tests.Dequeue();
}
