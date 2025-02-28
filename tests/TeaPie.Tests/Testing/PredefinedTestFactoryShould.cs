using NSubstitute;
using TeaPie.Http;
using TeaPie.Http.Headers;
using TeaPie.Testing;
using static Xunit.Assert;

namespace TeaPie.Tests.Testing;

public class PredefinedTestFactoryShould
{
    private readonly TestFactory _factory = new(Substitute.For<IHeadersHandler>());

    [Fact]
    public void ThrowExceptionForUnsupportedTestType()
    {
        var description = new PredefinedTestDescription((TestType)999);

        var exception = Throws<InvalidOperationException>(() => _factory.Create(description));
        Contains("Unable to create test for unsupported test type", exception.Message);
    }

    [Fact]
    public void CreateReturnExpectStatusCodesTest()
    {
        var description = new PredefinedTestDescription(TestType.ExpectStatusCodes, new int[] { 200, 201 });
        description.SetRequestExecutionContext(new RequestExecutionContext(null!));

        var test = _factory.Create(description);

        NotNull(test);
        Contains("Status code should be one of these", test.Name);
    }

    [Fact]
    public void CreateReturnHasBodyTest()
    {
        var description = new PredefinedTestDescription(TestType.HasBody, true);
        description.SetRequestExecutionContext(new RequestExecutionContext(null!));

        var test = _factory.Create(description);

        NotNull(test);
        Contains("Response should have body", test.Name);
    }

    [Fact]
    public void CreateReturnHasHeaderTest()
    {
        var description = new PredefinedTestDescription(TestType.HasHeader, "Authorization");
        description.SetRequestExecutionContext(new RequestExecutionContext(null!));

        var test = _factory.Create(description);

        NotNull(test);
        Contains("Response should have header with name 'Authorization'", test.Name);
    }
}
