using FluentAssertions;

namespace TeaPie.Tests;

public class ConstantsShould
{
    [Fact]
    public void HaveCorrectRequestFileExtension() =>
        Constants.RequestFileExtension.Should().Be(".http");

    [Fact]
    public void HaveCorrectScriptFileExtension() =>
        Constants.ScriptFileExtension.Should().Be(".csx");

    [Fact]
    public void HaveCorrectPreRequestSuffix() =>
        Constants.PreRequestSuffix.Should().Be("-init");

    [Fact]
    public void HaveCorrectRequestSuffix() =>
        Constants.RequestSuffix.Should().Be("-req");

    [Fact]
    public void HaveCorrectPostResponseSuffix() =>
        Constants.PostResponseSuffix.Should().Be("-test");

    [Fact]
    public void HaveCorrectDefaultEnvironmentFileName() =>
        Constants.DefaultEnvironmentFileName.Should().Be("env");

    [Fact]
    public void HaveCorrectEnvironmentFileExtension() =>
        Constants.EnvironmentFileExtension.Should().Be(".json");

    [Fact]
    public void HaveCorrectDefaultEnvironmentName() =>
        Constants.DefaultEnvironmentName.Should().Be("$shared");

    [Fact]
    public void HaveCorrectPascalCasePattern() =>
        Constants.PascalCasePattern.Should().Be("([A-Z][a-z]*|[a-z]+)");

    [Fact]
    public void HaveCorrectApplicationName() =>
        Constants.ApplicationName.Should().Be("TeaPie");

    [Fact]
    public void HaveCorrectDefaultInitializationScriptName() =>
        Constants.DefaultInitializationScriptName.Should().Be("init");

    [Fact]
    public void HaveCorrectTeaPieFolderName() =>
        Constants.TeaPieFolderName.Should().Be(".teapie");

    [Fact]
    public void HaveCorrectUnixEndOfLine() =>
        Constants.UnixEndOfLine.Should().Be("\n");

    [Fact]
    public void HaveCorrectWindowsEndOfLine() =>
        Constants.WindowsEndOfLine.Should().Be("\r\n");

    [Fact]
    public void HaveCorrectSecretVariableTag() =>
        Constants.SecretVariableTag.Should().Be("secret");

    [Fact]
    public void HaveCorrectNoCacheVariableTag() =>
        Constants.NoCacheVariableTag.Should().Be("no-cache");
}
