using FluentAssertions;

namespace TeaPie.Tests;

public class ApplicationContextOptionsBuilderShould
{
    [Fact]
    public void Build_ReturnsDefaultOptions_WhenNothingSet()
    {
        var options = new ApplicationContextOptionsBuilder().Build();

        options.TempFolderPath.Should().Be(Constants.SystemTemporaryFolderPath);
        options.Environment.Should().Be(string.Empty);
        options.EnvironmentFilePath.Should().Be(string.Empty);
        options.ReportFilePath.Should().Be(string.Empty);
        options.InitializationScriptPath.Should().Be(string.Empty);
        options.CacheVariables.Should().BeTrue();
    }

    [Fact]
    public void SetEnvironment_SetsEnvironment() =>
        new ApplicationContextOptionsBuilder()
            .SetEnvironment("dev")
            .Build()
            .Environment.Should().Be("dev");

    [Fact]
    public void SetReportFilePath_SetsReportFilePath() =>
        new ApplicationContextOptionsBuilder()
            .SetReportFilePath("report.xml")
            .Build()
            .ReportFilePath.Should().Be("report.xml");

    [Fact]
    public void SetVariablesCaching_SetsCachingFlag() =>
        new ApplicationContextOptionsBuilder()
            .SetVariablesCaching(false)
            .Build()
            .CacheVariables.Should().BeFalse();

    [Fact]
    public void EachSetter_ReturnsBuilder_ForFluentChaining()
    {
        var builder = new ApplicationContextOptionsBuilder();
        builder.SetEnvironment("dev").Should().BeSameAs(builder);
    }

    [Fact]
    public void SetTempFolderPath_WithNull_UsesEmptyString() =>
        new ApplicationContextOptionsBuilder()
            .SetTempFolderPath(null)
            .Build()
            .TempFolderPath.Should().Be(string.Empty);

    [Fact]
    public void SetEnvironment_WithNull_UsesEmptyString() =>
        new ApplicationContextOptionsBuilder()
            .SetEnvironment(null)
            .Build()
            .Environment.Should().Be(string.Empty);

    [Fact]
    public void Build_WithNoTempPath_UsesConstantsSystemTemporaryFolderPath() =>
        new ApplicationContextOptionsBuilder()
            .Build()
            .TempFolderPath.Should().Be(Constants.SystemTemporaryFolderPath);
}
