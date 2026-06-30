using FluentAssertions;

namespace TeaPie.Tests;

public class ExitCodeShould
{
    [Fact]
    public void HaveSuccessEqualToZero() =>
        ((int)ExitCode.Success).Should().Be(0);

    [Fact]
    public void HaveGeneralErrorEqualToOne() =>
        ((int)ExitCode.GeneralError).Should().Be(1);

    [Fact]
    public void HaveTestsFailedEqualToTwo() =>
        ((int)ExitCode.TestsFailed).Should().Be(2);

    [Fact]
    public void HaveCanceledEqualTo130() =>
        ((int)ExitCode.Canceled).Should().Be(130);
}
