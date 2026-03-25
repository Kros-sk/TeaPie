using TeaPie.TestCases;
using static Xunit.Assert;

namespace TeaPie.Tests.TestCases;

public class TpFileParserShould
{
    private readonly TpFileParser _parser = new();

    [Fact]
    public void ParseSingleTestCaseWithAllSections()
    {
        var content = """
            --- TESTCASE Add Customer

            --- INIT
            tp.SetVariable("X", 1);

            --- HTTP
            POST {{ApiBaseUrl}}/customers
            Content-Type: application/json

            {"Id": 1}

            --- TEST
            tp.Test("Response has body", () => NotNull(tp.Response.Content));

            --- END
            """;

        var result = _parser.Parse(content, "fallback");

        Single(result);
        var def = result[0];

        Equal("Add Customer", def.Name);
        NotNull(def.InitContent);
        Contains("tp.SetVariable", def.InitContent);
        Contains("POST {{ApiBaseUrl}}/customers", def.HttpContent);
        NotNull(def.TestContent);
        Contains("tp.Test", def.TestContent);
    }

    [Fact]
    public void ParseSingleTestCaseWithHttpOnly()
    {
        var content = """
            --- TESTCASE Health Check

            --- HTTP
            GET {{ApiBaseUrl}}/health

            --- END
            """;

        var result = _parser.Parse(content, "fallback");

        Single(result);
        var def = result[0];

        Equal("Health Check", def.Name);
        Null(def.InitContent);
        Contains("GET {{ApiBaseUrl}}/health", def.HttpContent);
        Null(def.TestContent);
    }

    [Fact]
    public void ParseMultipleTestCasesFromOneFile()
    {
        var content = """
            --- TESTCASE First Test

            --- HTTP
            GET {{ApiBaseUrl}}/first

            --- END

            --- TESTCASE Second Test

            --- HTTP
            POST {{ApiBaseUrl}}/second
            Content-Type: application/json

            {}

            --- TEST
            tp.Test("check", () => NotNull(tp.Response.Content));

            --- END
            """;

        var result = _parser.Parse(content, "fallback");

        Equal(2, result.Count);

        Equal("First Test", result[0].Name);
        Contains("GET {{ApiBaseUrl}}/first", result[0].HttpContent);
        Null(result[0].InitContent);
        Null(result[0].TestContent);

        Equal("Second Test", result[1].Name);
        Contains("POST {{ApiBaseUrl}}/second", result[1].HttpContent);
        Null(result[1].InitContent);
        NotNull(result[1].TestContent);
    }

    [Fact]
    public void ParseImplicitSingleTestCaseWithoutTestCaseMarker()
    {
        var content = """
            --- HTTP
            GET {{ApiBaseUrl}}/health

            --- TEST
            tp.Test("ok", () => Equal(200, tp.Response.StatusCode()));
            """;

        var result = _parser.Parse(content, "My Implicit Test");

        Single(result);
        var def = result[0];

        Equal("My Implicit Test", def.Name);
        Contains("GET {{ApiBaseUrl}}/health", def.HttpContent);
        NotNull(def.TestContent);
        Contains("tp.Test", def.TestContent);
        Null(def.InitContent);
    }

    [Fact]
    public void ThrowWhenHttpSectionIsMissing()
    {
        var content = """
            --- TESTCASE Bad Test

            --- INIT
            tp.SetVariable("x", 1);

            --- END
            """;

        var ex = Throws<InvalidOperationException>(() => _parser.Parse(content, "fallback"));
        Contains("Bad Test", ex.Message);
        Contains(TpConstants.HttpMarker, ex.Message);
    }

    [Fact]
    public void UseFallbackNameWhenSingleTestCaseMarkerHasNoName()
    {
        var content = """
            --- TESTCASE

            --- HTTP
            GET {{ApiBaseUrl}}/health

            --- END
            """;

        var result = _parser.Parse(content, "My Fallback Name");

        Single(result);
        Equal("My Fallback Name", result[0].Name);
    }

    [Fact]
    public void ThrowWhenMultipleTestCasesHaveNamelessMarker()
    {
        var content = """
            --- TESTCASE

            --- HTTP
            GET {{ApiBaseUrl}}/first

            --- END

            --- TESTCASE Second Test

            --- HTTP
            GET {{ApiBaseUrl}}/second

            --- END
            """;

        var ex = Throws<InvalidOperationException>(() => _parser.Parse(content, "fallback"));
        Contains(TpConstants.TestCaseMarker, ex.Message);
    }

    [Fact]
    public void HandleEmptyInitAndTestSections()
    {
        var content = """
            --- TESTCASE Empty Scripts

            --- INIT

            --- HTTP
            GET {{ApiBaseUrl}}/health

            --- TEST

            --- END
            """;

        var result = _parser.Parse(content, "fallback");

        Single(result);
        var def = result[0];

        Equal("Empty Scripts", def.Name);
        NotNull(def.HttpContent);
        NotEmpty(def.HttpContent);
        // Empty sections become empty strings after trim - treat as null-like
        True(def.InitContent is null || def.InitContent.Length == 0);
        True(def.TestContent is null || def.TestContent.Length == 0);
    }

    [Fact]
    public void PreserveFormattingOfHttpSection()
    {
        var httpBody = "{\"Id\": 1, \"Name\": \"Alice\"}";
        var content = string.Join("\n",
            "--- TESTCASE Format Test",
            "",
            "--- HTTP",
            "POST {{ApiBaseUrl}}/customers",
            "Content-Type: application/json",
            "",
            httpBody,
            "",
            "--- END");

        var result = _parser.Parse(content, "fallback");

        Single(result);
        Contains(httpBody, result[0].HttpContent);
        Contains("Content-Type: application/json", result[0].HttpContent);
    }

    [Fact]
    public void ParseCaseInsensitiveSectionMarkers()
    {
        var content = """
            --- testcase Case Insensitive

            --- http
            GET {{ApiBaseUrl}}/health

            --- test
            tp.Test("ok", () => Equal(200, tp.Response.StatusCode()));

            --- end
            """;

        var result = _parser.Parse(content, "fallback");

        Single(result);
        Equal("Case Insensitive", result[0].Name);
        NotNull(result[0].TestContent);
    }

    [Fact]
    public void ParseThreeTestCasesWithVariousConfigurations()
    {
        var content = """
            --- TESTCASE Init Only Test

            --- INIT
            tp.SetVariable("Counter", 0);

            --- HTTP
            GET {{ApiBaseUrl}}/counter

            --- END

            --- TESTCASE Full Test

            --- INIT
            var x = 1;

            --- HTTP
            POST {{ApiBaseUrl}}/items

            --- TEST
            tp.Test("check", () => NotNull(tp.Response.Content));

            --- END

            --- TESTCASE Simple Get

            --- HTTP
            GET {{ApiBaseUrl}}/items

            --- END
            """;

        var result = _parser.Parse(content, "fallback");

        Equal(3, result.Count);

        Equal("Init Only Test", result[0].Name);
        NotNull(result[0].InitContent);
        Null(result[0].TestContent);

        Equal("Full Test", result[1].Name);
        NotNull(result[1].InitContent);
        NotNull(result[1].TestContent);

        Equal("Simple Get", result[2].Name);
        Null(result[2].InitContent);
        Null(result[2].TestContent);
    }

    [Fact]
    public void ParseMultipleRequestsInHttpSection()
    {
        var content = """
            --- TESTCASE Multi Request

            --- HTTP
            # @name FirstRequest
            POST {{ApiBaseUrl}}/items
            Content-Type: application/json

            {"Id": 1}

            ###

            # @name SecondRequest
            GET {{ApiBaseUrl}}/items/{{FirstRequest.response.body.$.Id}}

            --- END
            """;

        var result = _parser.Parse(content, "fallback");

        Single(result);
        Contains("FirstRequest", result[0].HttpContent);
        Contains("SecondRequest", result[0].HttpContent);
        Contains("###", result[0].HttpContent);
    }
}
