using TeaPie.Http.Parsing;

namespace TeaPie.Testing;

internal static class TestDirectives
{
    public const string TestDirectivePrefix = "TEST-";
    public const string TestDirectiveParameterName = "DirectiveName";

    public const string TestDirectivePattern = @"^##\s*TEST-[A-Za-z0-9_-]+(?:\s*:\s*.+)?\s*$";

    public const string TestExpectStatusCodesDirectiveName = "EXPECT-STATUS";
    public const string TestExpectStatusCodesDirectiveFullName = TestDirectivePrefix + TestExpectStatusCodesDirectiveName;
    public const string TestExpectStatusCodesParameterName = "StatusCodes";
    public static readonly string TestExpectStatusCodesDirectivePattern =
        HttpDirectivePatternBuilder.Create(TestExpectStatusCodesDirectiveName)
            .WithPrefix(TestDirectivePrefix)
            .AddStatusCodesParameter(TestExpectStatusCodesParameterName)
            .Build();

    public const string TestHasBodyDirectiveName = "HAS-BODY";
    public const string TestHasBodyDirectiveFullName = TestDirectivePrefix + TestHasBodyDirectiveName;
    public const string TestHasBodyDirectiveParameterName = "Bool";
    public static readonly string TestHasBodyDirectivePattern =
        HttpDirectivePatternBuilder.Create(TestHasBodyDirectiveName)
            .WithPrefix(TestDirectivePrefix)
            .AddBooleanParameter(TestHasBodyDirectiveParameterName)
            .Build();

    public const string TestHasBodyNoParameterInternalDirectiveName = "HAS-BODY-SIMPLIFIED";
    public const string TestHasBodyNoParameterInternalDirectiveFullName =
        TestDirectivePrefix + TestHasBodyNoParameterInternalDirectiveName;
    public static readonly string TestHasBodyNoParameterDirectivePattern =
        HttpDirectivePatternBuilder.Create(TestHasBodyDirectiveName)
            .WithPrefix(TestDirectivePrefix)
            .Build();

    public const string TestHasHeaderDirectiveName = "HAS-HEADER";
    public const string TestHasHeaderDirectiveFullName = TestDirectivePrefix + TestHasHeaderDirectiveName;
    public const string TestHasHeaderDirectiveParameterName = "HeaderName";
    public static readonly string TestHasHeaderDirectivePattern =
        HttpDirectivePatternBuilder.Create(TestHasHeaderDirectiveName)
            .WithPrefix(TestDirectivePrefix)
            .AddHeaderNameParameter(TestHasHeaderDirectiveParameterName)
            .Build();
}
