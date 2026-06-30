using FluentAssertions;
using TeaPie.StructureExploration;

namespace TeaPie.Tests.StructureExploration;

public class ExternalFilesRegistryShould
{
    private readonly ExternalFilesRegistry _registry = new();

    [Fact]
    public void Register_And_Get_ReturnsTheElement()
    {
        var file = new ExternalFile("/path/to/file.txt");

        _registry.Register("myFile", file);

        _registry.Get("myFile").Should().Be(file);
    }

    [Fact]
    public void IsRegistered_ReturnsTrue_AfterRegistration()
    {
        var file = new ExternalFile("/path/to/file.txt");

        _registry.Register("myFile", file);

        _registry.IsRegistered("myFile").Should().BeTrue();
    }

    [Fact]
    public void IsRegistered_ReturnsFalse_BeforeRegistration()
    {
        _registry.IsRegistered("nonExistent").Should().BeFalse();
    }

    [Fact]
    public void Get_Throws_ForUnregisteredName()
    {
        var act = () => _registry.Get("unknown");

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Register_Overwrites_ExistingWithSameName()
    {
        var first = new ExternalFile("/path/first.txt");
        var second = new ExternalFile("/path/second.txt");

        _registry.Register("key", first);
        _registry.Register("key", second);

        _registry.Get("key").Should().Be(second);
    }
}
