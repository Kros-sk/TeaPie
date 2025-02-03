namespace TeaPie.Reporting;

public interface IReporter
{
    void Report();
}

public interface IReporter<TReportedObject>
{
    void Report(TReportedObject report);
}
