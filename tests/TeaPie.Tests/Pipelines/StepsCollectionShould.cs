using FluentAssertions;
using TeaPie.Pipelines;

namespace TeaPie.Tests.Pipelines;

public class StepsCollectionShould
{
    [Fact]
    public void ThrowProperExceptionWhenAddingNullStep()
    {
        var collection = new StepsCollection();
        collection.Invoking(coll => coll.Add(null!)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ThrowProperExceptionWhenAddingMultipleNullSteps()
    {
        var collection = new StepsCollection();
        collection.Invoking(coll => coll.AddRange([null!, null!, null!])).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddStepAtTheEndWhenUsingAddMethod()
    {
        var collection = new StepsCollection();
        var random = new Random();
        var numberOfElements = random.Next(1, 100);

        for (var i = 0; i < numberOfElements; i++)
        {
            collection.Add(new DummyStep());
        }

        var lastStep = new DummyStep();
        collection.Add(lastStep);

        var list = collection.ToList();

        list[^1].Should().Be(lastStep);
    }

    [Fact]
    public void ThrowProperExceptionWhenInsertingNullStep()
    {
        var collection = new StepsCollection();
        var firstStep = new DummyStep();
        collection.Add(firstStep);
        collection.Add(new DummyStep());

        collection.Invoking(coll => coll.Insert(firstStep, null!)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ThrowProperExceptionWhenInsertingMultipleNullSteps()
    {
        var collection = new StepsCollection();
        var firstStep = new DummyStep();
        collection.Add(firstStep);
        collection.Add(new DummyStep());

        collection.Invoking(coll => coll.InsertRange(firstStep, [null!, null!, null!])).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ThrowProperExceptionWhenInsertingStepAfterNullPredecessor()
    {
        var collection = new StepsCollection();
        var firstStep = new DummyStep();

        collection.Invoking(coll => coll.Insert(null!, firstStep)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ThrowProperExceptionWhenInsertingMultipleStepsAfterNullPredecessor()
    {
        var collection = new StepsCollection();
        var firstStep = new DummyStep();
        var secondStep = new DummyStep();

        collection.Invoking(coll => coll.InsertRange(null!, [firstStep, secondStep])).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ThrowProperExceptionWhenInsertingMultipleStepsAfterPredecessorOutOfCollection()
    {
        var collection = new StepsCollection();
        var firstStep = new DummyStep();

        collection.Invoking(coll => coll.InsertRange(firstStep, [new DummyStep(), new DummyStep(), new DummyStep()]))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void InsertMultipleStepsInCorrectOrder()
    {
        var collection = new StepsCollection();
        const int numberOfSteps = 7;
        var steps = new IdentifyingStep[numberOfSteps];

        var registerOfSteps = new List<int>();

        for (var i = 0; i < steps.Length; i++)
        {
            steps[i] = new(registerOfSteps, i);
        }

        collection.Add(steps[0]);
        collection.Add(steps[4]);
        collection.InsertRange(steps[0], [steps[1], steps[2], steps[3]]);
        collection.InsertRange(steps[4], [steps[5], steps[6]]);

        var list = collection.ToList();
        list.Count.Should().Be(numberOfSteps);

        for (var i = 0; i < numberOfSteps; i++)
        {
            ((IdentifyingStep)list[i]).Id.Should().Be(i);
        }
    }

    [Fact]
    public void HaveCollectionModificationResistantEnumeratorByDefault()
    {
        var collection = new StepsCollection();
        var enumerator = collection.GetEnumerator();
        const int numberOfStepsBeforeEnumeration = 7;
        const int numberOfEnumeratedStepsInFirstPartOfEnumeration = 5;
        const int numberOfStepsAfterEnumeration = 10;

        for (var i = 0; i < numberOfStepsBeforeEnumeration; i++)
        {
            collection.Add(new DummyStep());
        }

        var enumeratedCount = 0;
        for (var i = 0; i < numberOfEnumeratedStepsInFirstPartOfEnumeration; i++)
        {
            if (enumerator.MoveNext())
            {
                enumeratedCount++;
            }
            else
            {
                throw new Exception("There should be next element.");
            }
        }

        for (var i = 0; i < numberOfStepsAfterEnumeration - numberOfStepsBeforeEnumeration; i++)
        {
            collection.Add(new DummyStep());
        }

        while (enumerator.MoveNext())
        {
            enumeratedCount++;
        }

        enumeratedCount.Should().Be(numberOfStepsAfterEnumeration);
    }
}
