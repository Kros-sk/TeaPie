using Microsoft.Extensions.Logging;
using TeaPie.Reporting;

namespace TeaPie;

public interface IApplicationContext
{
    string Path { get; }

    string EnvironmentName { get; }

    ILogger Logger { get; }

    IServiceProvider ServiceProvider { get; }

    ITestResultsSummaryReporter Reporter { get; }
}
