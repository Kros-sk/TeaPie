using FluentAssertions;
using TeaPie.Functions;

namespace TeaPie.Tests.Functions;

public class FunctionNameViolationExceptionShould
{
    [Fact]
    public void BeCreatedWithDefaultConstructor()
    {
        var exception = new FunctionNameViolationException();
        exception.Should().NotBeNull();
    }

    [Fact]
    public void SetMessageWithMessageConstructor()
    {
        var exception = new FunctionNameViolationException("test message");
        exception.Message.Should().Be("test message");
    }

    [Fact]
    public void SetMessageAndInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var exception = new FunctionNameViolationException("outer", inner);
        exception.Message.Should().Be("outer");
        exception.InnerException.Should().BeSameAs(inner);
    }
}
