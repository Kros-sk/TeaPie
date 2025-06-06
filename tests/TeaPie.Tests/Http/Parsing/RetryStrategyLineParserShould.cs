﻿using TeaPie.Http.Parsing;
using TeaPie.Http.Retrying;
using static Xunit.Assert;

namespace TeaPie.Tests.Http.Parsing;

public class RetryStrategyLineParserShould
{
    private readonly HttpParsingContext _context = new(new HttpClient().DefaultRequestHeaders);
    private readonly RetryStrategyDirectiveLineParser _parser = new();

    [Fact]
    public void ParseRetryStrategyDirective()
    {
        const string line = "## RETRY-STRATEGY: FastRetry";

        True(_parser.CanParse(line, _context));
        _parser.Parse(line, _context);

        Equal("FastRetry", _context.RetryStrategyName);
    }

    [Fact]
    public void NotParseDifferentDirective()
    {
        const string line = "## INVALID-STRATEGY: SomethingElse";

        False(_parser.CanParse(line, _context));
    }
}
