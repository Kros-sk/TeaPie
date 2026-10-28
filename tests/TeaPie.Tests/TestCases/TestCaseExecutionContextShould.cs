using FluentAssertions;
using TeaPie.Templating;
using TeaPie.TestCases;

namespace TeaPie.Tests.TestCases;

public class TestCaseExecutionContextShould
{
    [Fact]
    public void OverwriteARequestAndResponseWithARepeatableTemplatedName()
    {
        var repeatable = new HashSet<string>(StringComparer.Ordinal) { "CreatePartner" };
        var context = new TestCaseExecutionContext(null!)
        {
            TemplatedRequestNames = new TemplatedRequestNames(repeatable, repeatable)
        };
        var secondRequest = new HttpRequestMessage();
        var secondResponse = new HttpResponseMessage();

        context.RegisterRequest(new HttpRequestMessage(), "CreatePartner");
        context.RegisterResponse(new HttpResponseMessage(), "CreatePartner");
        context.RegisterRequest(secondRequest, "CreatePartner");
        context.RegisterResponse(secondResponse, "CreatePartner");

        context.Requests["CreatePartner"].Should().Be(secondRequest);
        context.Responses["CreatePartner"].Should().Be(secondResponse);
    }

    [Fact]
    public void RejectARequestAndResponseWithTheSameNameByDefault()
    {
        var context = new TestCaseExecutionContext(null!);
        context.RegisterRequest(new HttpRequestMessage(), "CreatePartner");
        context.RegisterResponse(new HttpResponseMessage(), "CreatePartner");

        var registerRequest = () => context.RegisterRequest(new HttpRequestMessage(), "CreatePartner");
        var registerResponse = () => context.RegisterResponse(new HttpResponseMessage(), "CreatePartner");

        registerRequest.Should().Throw<ArgumentException>();
        registerResponse.Should().Throw<ArgumentException>();
    }
}
