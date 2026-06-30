using TeaPie.Pipelines;

namespace TeaPie.Telemetry;

internal class ReportTelemetrySummaryStep(ITelemetryCollector telemetryCollector) : IPipelineStep
{
    private readonly ITelemetryCollector _telemetryCollector = telemetryCollector;

    public bool ShouldExecute(ApplicationContext context) => true;

    public async Task Execute(ApplicationContext context, CancellationToken cancellationToken = default)
    {
        if (!_telemetryCollector.IsEnabled)
        {
            return;
        }

        var data = _telemetryCollector.GetTelemetryData();

        if (_telemetryCollector.Options.EnableConsoleOutput)
        {
            var consoleReporter = new SpectreConsoleTelemetryReporter();
            await consoleReporter.Report(data);
        }

        if (_telemetryCollector.Options.CustomProvider is not null)
        {
            await _telemetryCollector.Options.CustomProvider.OnCollectionCompleteAsync(data);
        }
    }
}
