using TeaPie.Http.Parsing;

namespace TeaPie.Http.Auth;
internal static class AuthDirectives
{
    public const string AuthDirectivePrefix = "AUTH-";

    public const string AuthProviderDirectiveName = "PROVIDER";
    public const string AuthProviderDirectiveFullName = AuthDirectivePrefix + AuthProviderDirectiveName;
    public const string AuthProviderDirectiveParameterName = "AuthProvider";
    public static readonly string AuthProviderSelectorDirectivePattern =
        HttpDirectivePatternBuilder.Create(AuthProviderDirectiveName)
            .WithPrefix(AuthDirectivePrefix)
            .AddStringParameter(AuthProviderDirectiveParameterName)
            .Build();
}
