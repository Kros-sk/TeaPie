using FluentAssertions;
using TeaPie.Http.Headers;

namespace TeaPie.Tests.Http.Headers;

public class ContentLanguageHeaderHandlerShould
{
    private readonly ContentLanguageHeaderHandler _handler = new();

    [Fact]
    public void SetHeader_AddsLanguage()
    {
        var request = new HttpRequestMessage { Content = new StringContent("body") };

        _handler.SetHeader("en-US", request);

        request.Content.Headers.ContentLanguage.Should().Contain("en-US");
    }

    [Fact]
    public void GetHeader_ReturnsLanguage()
    {
        var request = new HttpRequestMessage { Content = new StringContent("body") };
        request.Content.Headers.ContentLanguage.Add("en-US");

        _handler.GetHeader(request).Should().Be("en-US");
    }

    [Fact]
    public void SetHeader_Throws_WhenContentIsNull()
    {
        var request = new HttpRequestMessage();

        var act = () => _handler.SetHeader("en-US", request);

        act.Should().Throw<InvalidOperationException>();
    }
}
