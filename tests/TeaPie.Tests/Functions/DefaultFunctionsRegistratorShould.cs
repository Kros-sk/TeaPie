using FluentAssertions;
using TeaPie.Functions;

namespace TeaPie.Tests.Functions;

public class DefaultFunctionsRegistratorShould
{
    [Fact]
    public void RegisterNowFunction()
    {
        FunctionsCollection collection = [];

        DefaultFunctionsRegistrator.Register(collection);

        collection.Contains("$now", 1).Should().BeTrue();
    }

    [Fact]
    public void RegisterGuidFunction()
    {
        FunctionsCollection collection = [];

        DefaultFunctionsRegistrator.Register(collection);

        collection.Contains("$guid", 0).Should().BeTrue();
    }

    [Fact]
    public void RegisterRandFunction()
    {
        FunctionsCollection collection = [];

        DefaultFunctionsRegistrator.Register(collection);

        collection.Contains("$rand", 0).Should().BeTrue();
    }

    [Fact]
    public void RegisterRandomIntFunction()
    {
        FunctionsCollection collection = [];

        DefaultFunctionsRegistrator.Register(collection);

        collection.Contains("$randomInt", 2).Should().BeTrue();
    }

    [Fact]
    public void NowFunctionReturnsNonEmptyString()
    {
        FunctionsCollection collection = [];
        DefaultFunctionsRegistrator.Register(collection);

        var result = collection.Execute<string>("$now", "o");

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GuidFunctionReturnsValidGuid()
    {
        FunctionsCollection collection = [];
        DefaultFunctionsRegistrator.Register(collection);

        var result = collection.Execute<Guid>("$guid");

        result.Should().NotBe(Guid.Empty);
    }
}
