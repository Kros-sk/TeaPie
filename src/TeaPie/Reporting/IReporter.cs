namespace TeaPie.Reporting;

internal interface IReporter
{
    void Report();
}

internal interface IReporter<TReportedObject>
{
    void Report(TReportedObject report);
}
