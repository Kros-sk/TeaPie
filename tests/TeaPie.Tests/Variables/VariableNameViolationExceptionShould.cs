using FluentAssertions;
using TeaPie.Variables;

namespace TeaPie.Tests.Variables;

public class VariableNameViolationExceptionShould
{
    [Fact]
    public void BeCreatedWithDefaultConstructor()
    {
        var exception = new VariableNameViolationException();
        exception.Should().NotBeNull();
    }

    [Fact]
    public void SetMessageWithMessageConstructor()
    {
        var exception = new VariableNameViolationException("test message");
        exception.Message.Should().Be("test message");
    }

    [Fact]
    public void SetMessageAndInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var exception = new VariableNameViolationException("outer", inner);
        exception.Message.Should().Be("outer");
        exception.InnerException.Should().BeSameAs(inner);
    }
}
