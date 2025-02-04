using Microsoft.Extensions.Logging;

namespace TeaPie.Reporting;

public static class TeaPieReportingExtensions
{
    /// <summary>
    /// Registers a reporter to generate a test results summary at the end of the pipeline run.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="reporter">The reporter instance to be added to the collection of reporters.</param>
    public static void RegisterReporter(this TeaPie teaPie, IReporter<TestResultsSummary> reporter)
    {
        teaPie._reporter.RegisterReporter(reporter);
        teaPie.Logger.LogInformation(
            "Custom reporter of type {ReporterType} was successfully registered.", GetReportertTypeName(reporter));
    }

    /// <summary>
    /// Registers an inline reporter that report a test results summary using the specified <paramref name="onReportAction"/>.
    /// The report is generated at the end of the pipeline run.
    /// </summary>
    /// <param name="teaPie">The current context instance.</param>
    /// <param name="onReportAction">The action to be executed for test results summary report.</param>
    public static void RegisterReporter(this TeaPie teaPie, Action<TestResultsSummary> onReportAction)
    {
        teaPie._reporter.RegisterReporter(new InlineTestResultsSummaryReporter(onReportAction));
        teaPie.Logger.LogInformation("Custom reporter was successfully registered.");
    }

    private static string GetReportertTypeName(IReporter<TestResultsSummary> reporter)
    {
        var type = reporter.GetType();
        return type.IsGenericType
            ? FormatGenericTypeName(type)
            : type.Name;
    }

    private static string FormatGenericTypeName(Type type)
        => $"{type.GetGenericTypeDefinition().Name.Split('`')[0]}" +
            $"<{string.Join(", ", type.GetGenericArguments().Select(t => t.Name))}>";
}
