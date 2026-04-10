using FluentAssertions;

namespace TeaPie.Tests;

public class ApplicationContextOptionsShould
{
    [Fact]
    public void DefaultConstructor_SetsEmptyStringsAndCacheVariablesTrue()
    {
        var options = new ApplicationContextOptions();

        options.TempFolderPath.Should().Be(string.Empty);
        options.Environment.Should().Be(string.Empty);
        options.EnvironmentFilePath.Should().Be(string.Empty);
        options.ReportFilePath.Should().Be(string.Empty);
        options.InitializationScriptPath.Should().Be(string.Empty);
        options.CacheVariables.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithValues_SetsPropertiesCorrectly()
    {
        var options = new ApplicationContextOptions(
            "/temp", "prod", "env.json", "report.xml", "init.csx", false);

        options.TempFolderPath.Should().Be("/temp");
        options.Environment.Should().Be("prod");
        options.EnvironmentFilePath.Should().Be("env.json");
        options.ReportFilePath.Should().Be("report.xml");
        options.InitializationScriptPath.Should().Be("init.csx");
        options.CacheVariables.Should().BeFalse();
    }

    [Fact]
    public void NullParameters_DefaultToEmptyStrings()
    {
        var options = new ApplicationContextOptions(null, null, null, null, null);

        options.TempFolderPath.Should().Be(string.Empty);
        options.Environment.Should().Be(string.Empty);
        options.EnvironmentFilePath.Should().Be(string.Empty);
        options.ReportFilePath.Should().Be(string.Empty);
        options.InitializationScriptPath.Should().Be(string.Empty);
    }

    [Fact]
    public void CacheVariables_DefaultsToTrue() =>
        new ApplicationContextOptions().CacheVariables.Should().BeTrue();

    [Fact]
    public void Properties_AreSettable()
    {
        var options = new ApplicationContextOptions();
        options.TempFolderPath = "new";
        options.TempFolderPath.Should().Be("new");
    }
}
