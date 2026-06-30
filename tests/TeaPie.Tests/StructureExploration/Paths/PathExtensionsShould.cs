using FluentAssertions;
using TeaPie.StructureExploration.Paths;

namespace TeaPie.Tests.StructureExploration.Paths;

public class PathExtensionsShould
{
    [Fact]
    public void NormalizeSeparatorsToCurrentPlatform()
    {
        var path = "some/path\\to\\file";

        var result = path.NormalizeSeparators();

        result.Should().NotContain(Path.DirectorySeparatorChar == '/' ? "\\" : "/");
    }

    [Fact]
    public void TrimSlashAtTheEndRemovesTrailingSeparator()
    {
        var path = $"some{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}";

        var result = path.TrimSlashAtTheEnd();

        result.Should().NotEndWith(Path.DirectorySeparatorChar.ToString());
    }

    [Fact]
    public void TrimSlashInTheBeginningRemovesLeadingSeparator()
    {
        var path = $"{Path.DirectorySeparatorChar}some{Path.DirectorySeparatorChar}path";

        var result = path.TrimSlashInTheBeginning();

        result.Should().NotStartWith(Path.DirectorySeparatorChar.ToString());
    }

    [Fact]
    public void TrimSlashesRemovesBothLeadingAndTrailingSeparators()
    {
        var path = $"{Path.DirectorySeparatorChar}some{Path.DirectorySeparatorChar}path{Path.DirectorySeparatorChar}";

        var result = path.TrimSlashes();

        result.Should().NotStartWith(Path.DirectorySeparatorChar.ToString());
        result.Should().NotEndWith(Path.DirectorySeparatorChar.ToString());
    }

    [Theory]
    [InlineData("\"some/path\"", "some/path")]
    [InlineData("\"quoted\"", "quoted")]
    [InlineData("noquotes", "noquotes")]
    public void TrimQuotesRemovesSurroundingQuotes(string input, string expected)
    {
        var result = input.TrimQuotes();

        result.Should().Be(expected);
    }

    [Fact]
    public void NormalizePathTrimsAndRemovesQuotesAndNormalizesAndTrimsEndSlash()
    {
        var path = "  \"some/path/to/dir/\"  ";

        var result = path.NormalizePath();

        result.Should().NotStartWith(" ");
        result.Should().NotEndWith(Path.DirectorySeparatorChar.ToString());
        result.Should().NotContain("\"");
    }

    [Fact]
    public void TrimRootPathReturnsRelativePathWhenFullPathStartsWithRoot()
    {
        var rootPath = Path.Combine(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar), "root");
        var fullPath = Path.Combine(rootPath, "sub", "file.txt");

        var result = fullPath.TrimRootPath(rootPath);

        result.Should().Be(Path.Combine("sub", "file.txt"));
    }

    [Fact]
    public void TrimRootPathKeepsRootFolderNameWhenRequested()
    {
        var rootPath = Path.Combine(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar), "root");
        var fullPath = Path.Combine(rootPath, "sub", "file.txt");

        var result = fullPath.TrimRootPath(rootPath, keepRootFolder: true);

        result.Should().StartWith("root");
    }

    [Fact]
    public void TrimRootPathReturnsOriginalWhenPathDoesNotStartWithRoot()
    {
        var rootPath = Path.Combine(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar), "root");
        var fullPath = Path.Combine(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar), "other", "file.txt");

        var result = fullPath.TrimRootPath(rootPath);

        result.Should().Be(fullPath);
    }
}
